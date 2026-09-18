using System.Threading.Tasks;
using hexegeer.internallib;

namespace hexegeer {
	public sealed class HexegeerBattleModule : HexegeerModule<HexegeerBattleModule, HexegeerBattleModuleInternal> {
		protected override async Task LaunchModuleProcess() {
			Require<HexegeerWorldModule>();
			await Internal.Launch(HexegeerWorldModule.MasterDataEntity);
			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
			await Internal.Discard();
		}

		public static async Task Start() {
			if (IsActive) {
				await Internal.CreateModuleRunningTag();
			}
		}

		public static async Task Stop() {
			if (IsActive) {
				await Internal.DeleteModuleRunningTag();
			}
		}
	}
}