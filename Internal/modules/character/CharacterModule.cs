using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerWorldModuleSystemGroup))]
	public partial class HexegeerCharacterModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerCharacterModuleTag>();
		}
	}
	
	public struct HexegeerCharacterModuleTag : IComponentData { }

	public class HexegeerCharacterModuleInternal : HexegeerModuleInternal { 
		private Entity _characterTableEntity;

		public async Task Launch(Entity masterDataEntity) {
			CharacterTable table = await AssetUtil.RequestLoad<CharacterTable>(CharacterTable.RESOURCE_ADDRESS);
			SyncContext.Post(() => {
				CharacterBlobTable blobTable = new CharacterBlobTable { };

				blobTable.physicsObjectLayer = table.PhysicsObjectLayer;
				blobTable.physicsObjectCollides = table.PhysicsObjectCollides;

				// Master
				using (BlobBuilder characterBuilder = new BlobBuilder(Allocator.Temp)) {
					ref CharacterBlobAsset asset = ref characterBuilder.ConstructRoot<CharacterBlobAsset>();
					BlobBuilderArray<CharacterInfo> rows = characterBuilder.Allocate(ref asset.rows, table.Characters.Count);
					for (int i = 0; i < table.Characters.Count; ++i) {
						CharacterTable.Character row = table.Characters[i];
						CharacterModelLookup.Register(row);

						rows[i] = new CharacterInfo {
							id = row.id,
							name = row.name,
							collider = row.collider,
							belongsTo = row.belongsTo,
							collidesWith = row.collidesWith,
							hasObservationPoint = row.hasObservationPoint,
							eventAccessible = row.eventAccessible,
						};
					}
					
					BlobBuilderArray<CharacterHitAreaListAsset> hitArea = characterBuilder.Allocate(ref asset.hitArea, table.Characters.Count);
					for (int i = 0; i < table.Characters.Count; ++i) {
						BlobBuilderArray<CharacterHitArea> area = characterBuilder.Allocate(ref hitArea[i].list, table.Characters[i].hitAreas.Count);
						for (int j = 0; j < table.Characters[i].hitAreas.Count; ++j) {
							CharacterTable.HitArea areaInfo = table.Characters[i].hitAreas[j];
							area[j] = new CharacterHitArea {
								shape = areaInfo.shape,
								extent = areaInfo.extent,
								position = areaInfo.position,
								rotation = areaInfo.rotation,
							};
						}
					}

					blobTable.character = characterBuilder.CreateBlobAssetReference<CharacterBlobAsset>(Allocator.Persistent);

				}

				// Collider
				using (BlobBuilder colliderBuilder = new BlobBuilder(Allocator.Temp)) {
					ref CharacterColliderBlobAsset asset = ref colliderBuilder.ConstructRoot<CharacterColliderBlobAsset>();
					BlobBuilderArray<CharacterColliderInfo> rows = colliderBuilder.Allocate(ref asset.rows, table.Colliders.Count);
					for (int i = 0; i < table.Colliders.Count; ++i) {
						CharacterTable.CharacterCollider row = table.Colliders[i];
						rows[i] = new CharacterColliderInfo {
							id = row.id,
							name = row.name,
							radius = row.radius,
							height = row.height,
						};
					}
					blobTable.collider = colliderBuilder.CreateBlobAssetReference<CharacterColliderBlobAsset>(Allocator.Persistent);
				}

				// KeyTable
				using (BlobBuilder loadTableBuilder = new BlobBuilder(Allocator.Temp)) {
					ref CharacterLoadTableBlobAsset tableAsset = ref loadTableBuilder.ConstructRoot<CharacterLoadTableBlobAsset>();
					BlobBuilderArray<CharacterLoadListAsset> rows = loadTableBuilder.Allocate(ref tableAsset.rows, table.KeyTables.Length);
					for (int i = 0; i < table.KeyTables.Length; ++i) {
						CharacterTable.KeyTable keyTable = table.KeyTables[i];
						BlobBuilderArray<CharacterLoadElement> list = loadTableBuilder.Allocate(ref rows[i].list, keyTable.characterIndices.Count);
						rows[i].key = keyTable.key;
						for (int j = 0; j < keyTable.characterIndices.Count; ++j) {
							list[j] = new CharacterLoadElement { index = keyTable.characterIndices[j], };
						}
					}
					blobTable.loadTable = loadTableBuilder.CreateBlobAssetReference<CharacterLoadTableBlobAsset>(Allocator.Persistent);
				}

				EntityManager entityManager = ECS.EntityManager;
				_characterTableEntity = entityManager.Create(
					blobTable,
					new Parent { Value = masterDataEntity, },
					new LocalToWorld{ Value = float4x4.identity, },
					LocalTransform.Identity
				);

				AssetUtil.Release(CharacterTable.RESOURCE_ADDRESS);

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
			await CreateInstance(new HexegeerCharacterModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerCharacterModuleTag>();
		}

		public void ReleaseResources() {
			Application.quitting -= ReleaseResources;

			EntityManager entityManager = ECS.EntityManager;
			// Character Setting
			if (entityManager.Exists(_characterTableEntity)) {
				CharacterBlobTable table = entityManager.GetComponentData<CharacterBlobTable>(_characterTableEntity);
				table.character.Dispose();
				table.collider.Dispose();
				table.loadTable.Dispose();
				entityManager.DestroyEntity(_characterTableEntity);
				_characterTableEntity = Entity.Null;
			}
		}
	}
}
