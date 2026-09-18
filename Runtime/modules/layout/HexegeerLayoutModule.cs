using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer {
	public partial class HexegeerLayoutModule : HexegeerModule<HexegeerLayoutModule, HexegeerLayoutModuleInternal> {
		private Entity _layoutTableEntity;

		protected override async Task LaunchModuleProcess() {
			Require<HexegeerFieldModule>();
			Require<HexegeerWorldModule>();
			Require<HexegeerCharacterModule>();
			Require<HexegeerEventModule>();

			LayoutTable table = await AssetUtil.RequestLoad<LayoutTable>(LayoutTable.RESOURCE_ADDRESS);

			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;
				Entity masterDataEntity = HexegeerWorldModule.MasterDataEntity;

				using (BlobBuilder layoutBuilder = new BlobBuilder(Allocator.Temp)) {
					ref LayoutBlobAsset asset = ref layoutBuilder.ConstructRoot<LayoutBlobAsset>();
					BlobBuilderArray<LayoutProfile> rows = layoutBuilder.Allocate(ref asset.rows, table.LayoutProfiles.Count);
					for (int i = 0; i < table.LayoutProfiles.Count; ++i) {
						LayoutTable.Profile profile = table.LayoutProfiles[i];

						rows[i].contentKey = profile.ContentKey;

						BlobBuilderArray<LayoutLoadCharacterInfo> loadCharacters = layoutBuilder.Allocate(ref rows[i].loadCharacters, profile.CharacterIds.Count);
						for (int j = 0; j < profile.CharacterIds.Count; ++j) {
							loadCharacters[j] = new LayoutLoadCharacterInfo {
								id = profile.CharacterIds[j],
							};
						}

						BlobBuilderArray<LayoutCharacterInfo> characterLayout = layoutBuilder.Allocate(ref rows[i].characterLayout, profile.Characters.Count);
						for (int j = 0; j < profile.Characters.Count; ++j) {
							characterLayout[j] = new LayoutCharacterInfo {
								id = profile.Characters[j].Id,
								position = profile.Characters[j].Position,
								rotation = profile.Characters[j].Rotation,
							};
						}

						BlobBuilderArray<LayoutEventInfo> eventLayout = layoutBuilder.Allocate(ref rows[i].eventLayout, profile.Events.Count);
						for (int j = 0; j < profile.Events.Count; ++j) {
							eventLayout[j] = new LayoutEventInfo {
								eventId = profile.Events[j].EventId,
								position = profile.Events[j].Position,
								rotation = profile.Events[j].Rotation,
								shape = profile.Events[j].Shape,
								extent = profile.Events[j].Extent,
							};
						}
					}

					LayoutBlobTable component = new LayoutBlobTable {
						eventCollideInfo = new EventCollideInfo {
							belongsTo = Layer.Terrain,
							collidesWith = Layer.PhysicsObject,
						},
						asset = layoutBuilder.CreateBlobAssetReference<LayoutBlobAsset>(Allocator.Persistent),
					};
					
					_layoutTableEntity = entityManager.Create(
						component,
						new Parent { Value = masterDataEntity, },
						new LocalToWorld{ Value = float4x4.identity, },
						LocalTransform.Identity
					);
					ECS.SetEntityName(entityManager, _layoutTableEntity, "Layout Table@Hexegeer");
				}

				AssetUtil.Release(LayoutTable.RESOURCE_ADDRESS);
				Application.quitting += ReleaseResources;
			});

			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			SyncContext.Post(() => {
				ReleaseResources();
			});
			await Internal.DeleteModuleTag();
		}

		private void ReleaseResources() {
			Application.quitting += ReleaseResources;
			EntityManager entityManager = ECS.EntityManager;
			if (entityManager.Exists(_layoutTableEntity)) {
				LayoutBlobTable table = entityManager.GetComponentData<LayoutBlobTable>(_layoutTableEntity);
				table.asset.Dispose();
				entityManager.DestroyEntity(_layoutTableEntity);
				_layoutTableEntity = Entity.Null;
			}
		}

	}
}