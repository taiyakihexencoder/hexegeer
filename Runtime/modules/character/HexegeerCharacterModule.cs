using System.Threading.Tasks;
using hexegeer.internallib;

namespace hexegeer {
	public sealed class HexegeerCharacterModule : HexegeerModule<HexegeerCharacterModule, HexegeerCharacterModuleInternal> {
		protected override async Task LaunchModuleProcess() {
			Require<HexegeerWorldModule>();
			await Internal.Launch(HexegeerWorldModule.MasterDataEntity);
			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
			await Internal.Discard();
		}
	}
}