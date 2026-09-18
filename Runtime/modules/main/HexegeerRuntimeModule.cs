using System.Threading.Tasks;
using hexegeer.internallib;
using UnityEngine;

namespace hexegeer {
	public class HexegeerRuntimeModule : HexegeerModule<HexegeerRuntimeModule, HexegeerRuntimeModuleInternal> {
		protected override async Task LaunchModuleProcess() {
			SyncContext.Post(() => {
				Application.quitting += ReleaseResources;
			});

			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
			SyncContext.Post(() => {
				ReleaseResources();
			});
		}

		private void ReleaseResources() {
			Application.quitting -= ReleaseResources;
			MasterDataLoader.DisposeAllTable();
		}
	}
}