using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerRuntimeModuleSystemGroup))]
	public partial class HexegeerBattleModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerBattleModuleTag>();
		}
	}

	[UpdateInGroup(typeof(HexegeerRuntimeModuleSystemGroup))]
	public partial class HexegeerBattleModuleRunningSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerBattleModuleRunningTag>();
		}
	}


	[UpdateInGroup(typeof(HexegeerRuntimeModuleAfterColliderSystemGroup))]
	public partial class HexegeerBattleModuleAfterColliderSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerBattleModuleRunningTag>();
		}
	}
	
	public struct HexegeerBattleModuleTag : IComponentData { }
	public struct HexegeerBattleModuleRunningTag : IComponentData { }

	public class HexegeerBattleModuleInternal : HexegeerModuleInternal { 
		private Entity _damageObjectTableEntity;

		public async Task Launch(Entity masterDataEntity) {
			DamageObjectTable table = await AssetUtil.RequestLoad<DamageObjectTable>(DamageObjectTable.RESOURCE_ADDRESS);

			SyncContext.Send(() => {
				EntityManager entityManager = ECS.EntityManager;

				DamageObjectBlobTable blobTable = new DamageObjectBlobTable();

				using (BlobBuilder builder = new BlobBuilder(Allocator.Temp)) {
					ref DamageObjectBlobAsset damageObject = ref builder.ConstructRoot<DamageObjectBlobAsset>();
					BlobBuilderArray<DamageObjectInfo> objectList = builder.Allocate(ref damageObject.objectList, table.DamageObjects.Count);
					for (int i = 0; i < table.DamageObjects.Count; ++i) {
						DamageObjectTable.DamageObject row = table.DamageObjects[i];
						DamageObjectModelLookup.Register(row);

						objectList[i] = new DamageObjectInfo {
							id = row.id,
							name = new FixedString64Bytes(row.name),
							collider = row.collider,
						};
					}

					BlobBuilderArray<DamageObjectColliderInfo> colliderList = builder.Allocate(ref damageObject.colliderList, table.Colliders.Count);
					for (int i = 0; i < table.Colliders.Count; ++i) {
						DamageObjectTable.DamageObjectCollider collider = table.Colliders[i];
						colliderList[i] = new DamageObjectColliderInfo {
							id = collider.id,
							collidesWith = collider.collidesWith,
							belongsTo = collider.belongsTo,
							extent = collider.extent,
							rotation = collider.rotation,
							shape = collider.shape,
						};
					}
					blobTable.damageObject = builder.CreateBlobAssetReference<DamageObjectBlobAsset>(Allocator.Persistent);
				}

				using (BlobBuilder builder = new BlobBuilder(Allocator.Temp)) {
					ref DamageObjectKeyListBlobAsset keyTable = ref builder.ConstructRoot<DamageObjectKeyListBlobAsset>();
					BlobBuilderArray<DamageObjectKeyList> damageObjectKeyList = builder.Allocate(ref keyTable.list, table.ContentKeyTable.Length);
					for (int i = 0; i < table.ContentKeyTable.Length; ++i) {
						DamageObjectTable.KeyTable damageObjectKeyTable = table.ContentKeyTable[i];
						damageObjectKeyList[i] = new DamageObjectKeyList {
							key = damageObjectKeyTable.key,
						};

						BlobBuilderArray<DamageObjectLoadElement> elements = builder.Allocate(ref damageObjectKeyList[i].elements, table.ContentKeyTable[i].indices.Count);
						for (int j = 0; j < table.ContentKeyTable[i].indices.Count; ++j) {
							elements[j] = new DamageObjectLoadElement {
								index = damageObjectKeyTable.indices[j],
							};
						}
					}
					blobTable.keyTable = builder.CreateBlobAssetReference<DamageObjectKeyListBlobAsset>(Allocator.Persistent);
				}

				_damageObjectTableEntity = entityManager.Create(
					blobTable,
					new Parent{ Value = masterDataEntity, },
					new LocalToWorld{ Value = float4x4.identity, },
					LocalTransform.Identity
				);
				ECS.SetEntityName(entityManager, _damageObjectTableEntity, "Damage Object Table@Hexegeer");

				AssetUtil.Release(DamageObjectTable.RESOURCE_ADDRESS);

				Application.quitting += ReleaseResources;
			});
		}

		public async Task Discard() {
			SyncContext.Post(() => {
				ReleaseResources();
			});
			await Task.Yield();
		}

		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerBattleModuleTag());
		}

		public async Task CreateModuleRunningTag() {
			await CreateInstance(new HexegeerBattleModuleRunningTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerBattleModuleTag>();
		}

		public async Task DeleteModuleRunningTag() {
			await DeleteInstance<HexegeerBattleModuleRunningTag>();
		}

		private void ReleaseResources() {
			Application.quitting -= ReleaseResources;
			EntityManager entityManager = ECS.EntityManager;

			if (entityManager.Exists(_damageObjectTableEntity)) {
				DamageObjectBlobTable table = entityManager.GetComponentData<DamageObjectBlobTable>(_damageObjectTableEntity);
				table.damageObject.Dispose();
				table.keyTable.Dispose();
				entityManager.DestroyEntity(_damageObjectTableEntity);
				_damageObjectTableEntity = Entity.Null;
			}
		}
	}
}
