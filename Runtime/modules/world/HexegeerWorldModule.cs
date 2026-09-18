using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Entities;

namespace hexegeer {
	public sealed class HexegeerWorldModule : HexegeerModule<HexegeerWorldModule, HexegeerWorldModuleInternal> {
		public static Entity MasterDataEntity => Internal.MasterDataEntity;

		protected override async Task LaunchModuleProcess() {
			Require<HexegeerRuntimeModule>();
			await Internal.Launch();
			await Internal.CreateModuleTag();

			// グローバルリソース読込リクエスト
			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;
				entityManager.Create(new ContentKeyLoadRequest{ contentKey = ContentKey.Global.value, });
			});
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
			await Internal.Discard();
		}

		public static void StartPhysics() {
			if (IsActive) {
				Internal.StartPhysics();
			}
		}

		public static void EndPhysics() {
			if (IsActive) {
				Internal.EndPhysics();
			}
		}
	}
}