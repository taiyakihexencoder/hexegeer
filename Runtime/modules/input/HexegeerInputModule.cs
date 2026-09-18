using System.Threading.Tasks;
using hexegeer.internallib;

namespace hexegeer {
	public sealed class HexegeerInputModule : HexegeerModule<HexegeerInputModule, HexegeerInputModuleInternal> {
		protected override async Task LaunchModuleProcess() {
			Require<HexegeerRuntimeModule>();
			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
		}
	}
}