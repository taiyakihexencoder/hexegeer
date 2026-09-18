using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerWorldModuleSystemGroup))]
	public partial class HexegeerFieldModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerFieldModuleTag>();
		}
	}
	
	public struct HexegeerFieldModuleTag : IComponentData { }

	public class HexegeerFieldModuleInternal : HexegeerModuleInternal { 
		private FieldLoader _loader;

		private Entity _fieldTableEntity = Entity.Null;
		private Entity _headersParentEntity = Entity.Null;

		public async Task Launch(Entity masterDataEntity) {
			FieldTable table = await AssetUtil.RequestLoad<FieldTable>(FieldTable.RESOURCE_ADDRESS);

			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;

				using (BlobBuilder builder = new BlobBuilder(Allocator.Temp)) {
					ref FieldBlobAsset asset = ref builder.ConstructRoot<FieldBlobAsset>();
					BlobBuilderArray<FieldInfo> rows = builder.Allocate(ref asset.rows, table.Rows.Length);
					for(int i = 0; i < table.Rows.Length; ++i) {
						FieldTable.Row row = table.Rows[i];
						rows[i] = new FieldInfo {
							id = row.id,
							contentKey = row.contentKey,
							address = row.address,
							name = row.name,
							guid = row.guid,
							position = row.position,
							rotation = row.rotation,
							boundsMin = row.boundsMin,
							boundsMax = row.boundsMax,
						};
					}
				
					FieldBlobTable blobTable = new FieldBlobTable {
						asset = builder.CreateBlobAssetReference<FieldBlobAsset>(Allocator.Persistent),
					};

					_fieldTableEntity = entityManager.Create(
						blobTable,
						new Parent{ Value = masterDataEntity, },
						new LocalToWorld{ Value = float4x4.identity, },
						LocalTransform.Identity
					);
					ECS.SetEntityName(entityManager, _fieldTableEntity, "Field Table@Hexegeer");

					CreateHeaders(entityManager, blobTable);
				}
				AssetUtil.Release(FieldTable.RESOURCE_ADDRESS);

				// Field Loader
				if (_loader == null) {
					EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
						.WithAll<FieldSetting>()
						.Build(entityManager);
					
					if (query.TryGetSingleton(out FieldSetting setting)) {
						_loader = new FieldLoader(
							entityManager: entityManager,
							material: Unity.Physics.Material.Default,
							filter:  new CollisionFilter {
								BelongsTo = setting.belongsTo,
								CollidesWith = setting.collidesWith,
							},
							cacheCount: setting.cacheFieldMeshCount
						);
					}
				}

				Application.quitting += ReleaseResources;
			});
		}

		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerFieldModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerFieldModuleTag>();
		}

		public async Task Discard() {
			SyncContext.Post(() => {
				ReleaseResources();
			});
			await Task.Yield();
		}

		private void CreateHeaders(EntityManager entityManager, FieldBlobTable table) {
			if (!entityManager.Exists(_headersParentEntity)) {
				_headersParentEntity = entityManager.Create(
					new Parent(),
					LocalTransform.Identity,
					new LocalToWorld { Value = float4x4.identity, },
					new AttachHexegeerTree()
				);
				ECS.SetEntityName(entityManager, _headersParentEntity, "Field@Hexegeer");
			}

			EntityArchetype headerArchetype = entityManager.CreateArchetype(
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<FieldHeader>(),
				ComponentType.ReadWrite<LinkedEntityGroup>()
			);

			for (int i = 0; i < table.asset.Value.rows.Length; ++i) {
				FieldInfo row = table.asset.Value.rows[i];
				FieldHeader header = new FieldHeader {
					active = false,
					id = row.id,
					contentKey = row.contentKey,
					boundsMin = row.boundsMin,
					boundsMax = row.boundsMax,
					lastUpdated = 0.0,
				};

				Entity entity = entityManager.CreateEntity(headerArchetype);
				ECS.SetComponents(
					entityManager,
					entity,
					LocalTransform.FromPositionRotation(row.position, row.rotation),
					new LocalToWorld { Value = float4x4.TRS(row.position, row.rotation, new float3(1f,1f,1f)), },
					new Parent { Value = _headersParentEntity, },
					header
				);
				ECS.SetEntityName(entityManager, entity, row.name);
			}
		}

		public void CreateFieldEntity(FieldBlobTable table, Entity header, int id, bool keep) {
			if (_loader == null) {
				D.LogW("FieldLoader instance not initialized.");
			} else {
				Task.Run(async () => {
					await _loader.CreateFieldEntity(table, header, id, keep);
				});
			}
		}

		private void ReleaseResources() {
			Application.quitting -= ReleaseResources;

			EntityManager entityManager = ECS.EntityManager;

			// FieldHeaderのEntityはLinkedEntityGroupを保持しているため、
			// EntityQueryから削除しようとするとエラーになる。
			EntityQuery headerQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<FieldHeader, LinkedEntityGroup>()
				.Build(entityManager);
			NativeArray<Entity> headerEntities = headerQuery.ToEntityArray(Allocator.Temp);
			foreach(Entity headerEntity in headerEntities) {
				entityManager.DestroyEntity(headerEntity);
			}
			headerEntities.Dispose();

			if (entityManager.Exists(_headersParentEntity)) {
				entityManager.DestroyEntity(_headersParentEntity);
				_headersParentEntity = Entity.Null;
			}


			if (entityManager.Exists(_fieldTableEntity)) {
				FieldBlobTable table = entityManager.GetComponentData<FieldBlobTable>(_fieldTableEntity);
				table.asset.Dispose();
				entityManager.DestroyEntity(_fieldTableEntity);
				_fieldTableEntity = Entity.Null;
			}

			_loader?.Clear();
		}
	}
}
