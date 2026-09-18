using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace hexegeer.internallib {
	public class FieldLoader {
		private struct FieldCache {
			public Entity header;
			public int id;
			public int contentKey;
			public List<MeshCache> meshList;
		}

		private struct MeshCache {
			public BlobAssetReference<Collider> collider;
			public string name;
		}

		private List<FieldCache> _caches;
		private List<int> _loadingList;
		private int _cacheLimit;

		// Mesh
		private Material _material;
		private EntityArchetype _meshArchetype;
		private CollisionFilter _filter;


		public FieldLoader(
			EntityManager entityManager, 
			Material material,
			CollisionFilter filter,
			int cacheCount
		) {
			_caches = new List<FieldCache>();
			_loadingList = new List<int>();

			_meshArchetype = entityManager.CreateArchetype(
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<PhysicsCollider>(),
				ComponentType.ReadWrite<FieldMesh>()
			);

			_material = material;
			_filter = filter;
			_cacheLimit = cacheCount;
		}

		public void Clear() {
			foreach(FieldCache cache in _caches) {
				foreach(MeshCache mesh in cache.meshList) {
					mesh.collider.Dispose();
				}
			}
			_caches.Clear();
			_loadingList.Clear();
		}

		public async Task CreateFieldEntity(FieldBlobTable table, Entity header, int id, bool keep) {
			int cacheIndex = _caches.FindIndex(_ => _.id == id);
			if (cacheIndex >= 0) {
				SyncContext.Post(() => {
					FieldCache cache = _caches[cacheIndex];
					_caches.Remove(cache);
					_caches.Add(cache);
					CreateFieldEntityFromCache(_caches[cacheIndex], keep);
				});
			} else {
				if (!_loadingList.Contains(id)) {
					_loadingList.Add(id);
					FieldCache? cache = await LoadField(table, header, id);
					if (cache != null) {
						SyncContext.Post(() => {
							while (_caches.Count >= _cacheLimit) {
								foreach(MeshCache mesh in _caches[0].meshList) {
									mesh.collider.Dispose();
								}
								_caches.RemoveAt(0);
							}
							_caches.Add(cache.Value);
							_loadingList.Remove(id);
							CreateFieldEntityFromCache(cache.Value, keep);
						});
					} else {
						D.LogW($"Field not found: id = {id}");
					}
				}
			}
		}

		private async Task<FieldCache?> LoadField(FieldBlobTable table, Entity header, int id) {
			int count = table.asset.Value.rows.Length;
			for (int i = 0; i < count; ++i) {
				FieldInfo fieldInfo = table.asset.Value.rows[i];
				if (fieldInfo.id == id) {
					List<MeshCache> meshes = await LoadMeshes(fieldInfo.address.ConvertToString());
					return new FieldCache {
						id = id,
						contentKey = fieldInfo.contentKey,
						header = header,
						meshList = meshes,
					};
				}
			}
			return null;
		}

		private async Task<List<MeshCache>> LoadMeshes(string address) {
			List<MeshCache> createList = new List<MeshCache>();
			string[] meshNames = new string[0];
			await AssetUtil.LoadTemp<FieldMeshResource>(address, scriptable => {
				meshNames = new string[scriptable.Subassets.Length];
				System.Array.Copy(scriptable.Subassets, meshNames, meshNames.Length);
			});
			
			List<UnityEngine.Mesh> meshes = await AssetUtil.RequestLoadSubAssets<UnityEngine.Mesh>(address, meshNames);
			
			SyncContext.Send(() => {
				foreach(UnityEngine.Mesh mesh in meshes) {
					BlobAssetReference<Collider> collider = SyncContext.Send(() => {
						return MeshCollider.Create(mesh, _filter, _material);
					});
					createList.Add(
						new MeshCache {
							collider = collider,
							name = mesh.name,
						}
					);
				}
			});
			return createList;
		}

		private void CreateFieldEntityFromCache(in FieldCache cache, bool keep) {
			EntityManager entityManager = ECS.EntityManager;

			FieldMesh fieldMesh = new FieldMesh{ meshId = cache.id, };
			LocalTransform localTransform = LocalTransform.FromPositionRotationScale(float3.zero, quaternion.identity, 1.0f);
			LocalToWorld localToWorld = new LocalToWorld{ Value = float4x4.identity, };
			Parent parent = new Parent { Value = cache.header, };
			PhysicsWorldIndex physicsWorldIndex = new PhysicsWorldIndex { Value = 0, };

			List<Entity> entities = new List<Entity>();
			foreach(MeshCache mesh in cache.meshList) {
				Entity entity = entityManager.CreateEntity(_meshArchetype);
				PhysicsCollider physicsCollider = new PhysicsCollider { Value = mesh.collider, };
				ECS.SetComponents(
					entityManager,
					entity,
					fieldMesh,
					localTransform,
					localToWorld,
					parent,
					physicsCollider
				);
				entityManager.AddSharedComponent(entity, physicsWorldIndex);
				ECS.SetEntityName(entityManager, entity, mesh.name);
				entities.Add(entity);
			}

			if (keep) {
				entityManager.AddComponent<FieldUnloadExclude>(cache.header);
			}

			// SharedComponentを追加して構造が変わるため、上のforeach内で追加するとエラーになる。
			// Entityの構造が変わらない最後に一括して追加する。
			DynamicBuffer<LinkedEntityGroup> group = entityManager.GetBuffer<LinkedEntityGroup>(cache.header);
			group.Clear();
			foreach(Entity entity in entities) {
				group.Add(new LinkedEntityGroup { Value = entity, });
			}

			// コンテンツの追加を依頼
			entityManager.Create(new ContentKeyLoadRequest { contentKey = cache.contentKey, });
		}
	}
}