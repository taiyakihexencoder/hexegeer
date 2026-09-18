using System.Threading.Tasks;
using hexegeer.internallib;

namespace hexegeer {
	public sealed class HexegeerDebugModule : HexegeerModule<HexegeerDebugModule, HexegeerDebugModuleInternal> {
		protected override async Task LaunchModuleProcess() {
			await Internal.Launch();
			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
			await Internal.Discard();
		}
	}
}