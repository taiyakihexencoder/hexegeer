using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;

namespace hexegeer {
	public sealed class HexegeerFieldModule : HexegeerModule<HexegeerFieldModule, HexegeerFieldModuleInternal> {
		protected override async Task LaunchModuleProcess() {
			Require<HexegeerRuntimeModule>();

			SyncContext.Send(() => {
				FieldSettingGenerator.Generate(ECS.EntityManager, HexegeerWorldModule.MasterDataEntity);
			});

			await Internal.Launch(HexegeerWorldModule.MasterDataEntity);
			await Internal.CreateModuleTag();

			// TODO: セーブデータから読み込んだEntryPointを設定する
			SyncContext.Post(() => {
				// Preload Entry Point
				float3 position = new float3(0f, 20f, 0f);
				EntityManager entityManager = ECS.EntityManager;
				entityManager.Create(
					LocalTransform.FromPosition(position),
					new LocalToWorld { Value = float4x4.Translate(position), },
					new FieldObservationPoint(),
					new FieldPreload()
				);
			});
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
			await Internal.Discard();

			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;
				EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
					.WithAll<FieldSetting>()
					.Build(entityManager);
				entityManager.DestroyEntity(query);
			});
		}

		public static void OnRequestCreateFieldEntity(FieldBlobTable table, Entity header, int id, bool keep) {
			Internal.CreateFieldEntity(table, header, id, keep);
		}
	}
}