using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace hexegeer.internallib {
	/// <summary>
	/// フィールドの読み込み
	/// </summary>
	[UpdateInGroup(typeof(HexegeerFieldInternalSystemGroup))]
	public partial class FieldLoadingSystem : SystemBase {
		private EntityQuery _requestQuery;
		private EntityQuery _queueQuery;

		private FieldTable _table;
		private bool _awaked;

		private List<EntityCreateInfo> _cacheList;
		private ConcurrentDictionary<int, Entity> _headerEntities;
		private ConcurrentQueue<EntityCreateInfo> _queue;

		private EntityArchetype _headerArchetype;
		private EntityArchetype _meshArchetype;
		private Material _material;

		private Entity _rootEntity;
		private Entity _loadQueueEntity;

		private struct EntityCreateInfo {
			public Entity headerEntity;
			public int id;
			public int contentKey;
			public List<MeshCreateInfo> meshList;
		}

		private struct MeshCreateInfo {
			public BlobAssetReference<Collider> collider;
			public string name;
		}

		protected override void OnCreate() {
			_material = Material.Default;

			_requestQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<FieldLoadRequest>()
				.Build(EntityManager);
			_queueQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<FieldLoadQueue>()
				.Build(EntityManager);

			RequireAnyForUpdate(_requestQuery, _queueQuery);

			_awaked = false;
			
			_cacheList = new List<EntityCreateInfo>();
			_headerEntities = new ConcurrentDictionary<int, Entity>();

			_queue = new ConcurrentQueue<EntityCreateInfo>();

			_headerArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<FieldHeader>(),
				ComponentType.ReadWrite<LinkedEntityGroup>()
			);

			_meshArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<PhysicsCollider>(),
				ComponentType.ReadWrite<FieldMesh>()
			);

			_rootEntity = EntityManager.Create(
				new Parent(),
				LocalTransform.Identity,
				new LocalToWorld { Value = float4x4.identity, },
				new AttachHexegeerTree()
			);
			ECS.SetEntityName(EntityManager, _rootEntity, "Field@Hexegeer");

			_loadQueueEntity = EntityManager.Create(
				new FieldLoadQueue(),
				LocalTransform.Identity,
				new LocalToWorld { Value = float4x4.identity, },
				new Parent{ Value = _rootEntity, }
			);
			ECS.SetEntityName(EntityManager, _loadQueueEntity, "Field Load Queue@Hexegeer");
			EntityManager.SetEnabled(_loadQueueEntity, false);

			_table = null;
		}

		protected override void OnDestroy() {
			if (_table != null) {
				AssetUtil.Release(FieldTable.RESOURCE_ADDRESS);
			}
			SetCacheCount(0);
		}

		protected override void OnStartRunning() {
			if(!_awaked) {
				_awaked = true;

				// 初期化処理
				Task.Run( async () => _table = await LoadFieldTable() );
			}
		}

		protected override void OnUpdate() {
			if (! SystemAPI.TryGetSingleton(out FieldSetting settings)) { return; }
			if (_table == null) { return; }

			NativeArray<FieldLoadRequest> requests = _requestQuery.ToComponentDataArray<FieldLoadRequest>(Allocator.Temp);

			foreach(FieldLoadRequest request in requests) {
				int id = request.id;

				int index = _cacheList.FindIndex(_ => _.id == id);
				if (index >= 0) {
					EntityCreateInfo info = _cacheList[index];
					CreateMeshEntities(info, settings.cacheFieldMeshCount);
				} else {
					// queueへの追加待機
					EntityManager.SetEnabled(_loadQueueEntity, true);

					Entity parent = Entity.Null;
					if (!_headerEntities.TryGetValue(request.id, out parent)) {
						parent = Entity.Null;
					}

					Task.Run( async () => {
						await LoadFieldMesh(
							parent, 
							id, 
							settings.belongsTo, 
							settings.collidesWith
						);
					});
				}
			}

			while( _queue.TryDequeue(out EntityCreateInfo createInfo) ) {
				CreateMeshEntities(createInfo, settings.cacheFieldMeshCount);
				if (_queue.IsEmpty) {
					EntityManager.SetEnabled(_loadQueueEntity, false);
				}
			}

			// リクエストを破棄
			EntityManager.DestroyEntity(_requestQuery);
		}

		private async Task<FieldTable> LoadFieldTable() {
			FieldTable table = await AssetUtil.RequestLoad<FieldTable>(FieldTable.RESOURCE_ADDRESS);

			SyncContext.Send(() => {
				foreach(FieldTable.Row row in table.Rows) {
					FieldHeader header = new FieldHeader {
						active = false,
						id = row.id,
						contentKey = row.contentKey,
						boundsMin = row.boundsMin,
						boundsMax = row.boundsMax,
						lastUpdated = 0.0,
					};

					Entity entity = EntityManager.CreateEntity(_headerArchetype);
					ECS.SetComponents(
						EntityManager,
						entity,
						LocalTransform.FromPositionRotation(row.position, row.rotation),
						new LocalToWorld { Value = float4x4.TRS(row.position, row.rotation, new float3(1f,1f,1f)), },
						new Parent { Value = _rootEntity, },
						header
					);
					ECS.SetEntityName(EntityManager, entity, row.name);

					_headerEntities.TryAdd(row.id, entity);
				}
			});

			return table;
		}

		private async Task LoadFieldMesh(Entity parent, int id, uint belongsTo, uint collidesWith) {
			// フィールドテーブルの読み込み待ち
			while(_table == null) { await Task.Yield(); }

			CollisionFilter filter = new CollisionFilter {
				BelongsTo = belongsTo,
				CollidesWith = collidesWith,
			};

			foreach(FieldTable.Row row in _table.Rows) {
				if (row.id == id) {
					List<MeshCreateInfo> meshes = await LoadMeshes(row.address, filter);
					_queue.Enqueue(
						new EntityCreateInfo {
							id = id,
							contentKey = row.contentKey,
							headerEntity = parent,
							meshList = meshes,
						}
					);
					break;
				}
			}
		}

		private async Task<List<MeshCreateInfo>> LoadMeshes(string address, CollisionFilter filter) {
			List<MeshCreateInfo> createList = new List<MeshCreateInfo>();
			string[] meshNames = new string[0];
			await AssetUtil.LoadTemp<FieldMeshResource>(address, scriptable => {
				meshNames = new string[scriptable.Subassets.Length];
				System.Array.Copy(scriptable.Subassets, meshNames, meshNames.Length);
			});
			
			List<UnityEngine.Mesh> meshes = await AssetUtil.RequestLoadSubAssets<UnityEngine.Mesh>(address, meshNames);
			
			SyncContext.Send(() => {
				foreach(UnityEngine.Mesh mesh in meshes) {
					BlobAssetReference<Collider> collider = SyncContext.Send(() => {
						return MeshCollider.Create(mesh, filter, _material);
					});
					createList.Add(
						new MeshCreateInfo{
							collider = collider,
							name = mesh.name,
						}
					);
				}
			});
			return createList;
		}

		private void CreateMeshEntities(in EntityCreateInfo createInfo, int cacheCount) {
			int id = createInfo.id;
			_cacheList.RemoveAll(_ => _.id == id);
			_cacheList.Add(createInfo);
			SetCacheCount(cacheCount);

			FieldMesh fieldMesh = new FieldMesh{ meshId = id, };
			LocalTransform localTransform = LocalTransform.FromPositionRotationScale(float3.zero, quaternion.identity, 1.0f);
			LocalToWorld localToWorld = new LocalToWorld{ Value = float4x4.identity, };
			Parent parent = new Parent { Value = createInfo.headerEntity, };
			PhysicsWorldIndex physicsWorldIndex = new PhysicsWorldIndex { Value = 0, };

			List<Entity> entities = new List<Entity>();
			foreach(MeshCreateInfo mesh in createInfo.meshList) {
				Entity entity = EntityManager.CreateEntity(_meshArchetype);
				PhysicsCollider physicsCollider = new PhysicsCollider { Value = mesh.collider, };
				ECS.SetComponents(
					EntityManager,
					entity,
					fieldMesh,
					localTransform,
					localToWorld,
					parent,
					physicsCollider
				);
				EntityManager.AddSharedComponent(entity, physicsWorldIndex);
				ECS.SetEntityName(EntityManager, entity, mesh.name);
				entities.Add(entity);
			}

			// SharedComponentを追加して構造が変わるため、上のforeach内で追加するとエラーになる。
			// Entityの構造が変わらない最後に一括して追加する。
			DynamicBuffer<LinkedEntityGroup> group = EntityManager.GetBuffer<LinkedEntityGroup>(createInfo.headerEntity);
			group.Clear();
			foreach(Entity entity in entities) {
				group.Add(new LinkedEntityGroup { Value = entity, });
			}

			// コンテンツの追加を依頼
			EntityManager.Create(new ContentKeyLoadRequest { contentKey = createInfo.contentKey, });
		}
		
		private void SetCacheCount(int cacheCount) {
			// キャッシュ数が超過してしまう場合は破棄する。
			while(_cacheList.Count > cacheCount) {
				EntityCreateInfo info = _cacheList[0];

				// メッシュの破棄
				foreach(MeshCreateInfo mesh in info.meshList) {
					mesh.collider.Dispose();
				}

				_cacheList.RemoveAt(0);
			}
		}
	}
}