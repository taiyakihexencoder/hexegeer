using System.Threading.Tasks;
using hexegeer.internallib;

namespace hexegeer {
	public sealed class HexegeerEventModule : HexegeerModule<HexegeerEventModule, HexegeerEventModuleInternal> {
		protected override async Task LaunchModuleProcess() {
			Require<HexegeerWorldModule>();
			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
		}

		public static async Task Start() {
			await Internal.CreateModuleRunningTag();
		}

		public static async Task Stop() {
			await Internal.DeleteModuleRunningTag();
		}
	}
}