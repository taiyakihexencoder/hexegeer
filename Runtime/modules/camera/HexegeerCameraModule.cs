using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Entities;

namespace hexegeer {
	public sealed class HexegeerCameraModule : HexegeerModule<HexegeerCameraModule, HexegeerCameraModuleInternal> {
		public static Entity CameraEntity => Internal.CameraEntity;

		protected override async Task LaunchModuleProcess() {
			Require<HexegeerWorldModule>();

			await Internal.Launch();
			await Internal.CreateModuleTag();
		}
		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
			await Internal.Discard();
		}
	}
}