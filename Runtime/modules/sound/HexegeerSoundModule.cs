using System.Threading.Tasks;
using hexegeer.internallib;

namespace hexegeer {
	public sealed class HexegeerSoundModule : HexegeerModule<HexegeerSoundModule, HexegeerSoundModuleInternal> {
		public static MusicTable MusicTable => Internal.MusicTable;
		public static MusicPlayer MusicPlayer => Internal.MusicPlayer;

		public static EnvironmentSoundTable EnvironmentSoundTable => Internal.EnvironmentSoundTable;
		public static SystemSoundTable SystemSoundTable => Internal.SystemSoundTable;
		public static SoundPlayer SoundPlayer => Internal.SoundPlayer;

		protected override async Task LaunchModuleProcess() {
			Require<HexegeerRuntimeModule>();
			await Internal.Launch();
			await Internal.CreateModuleTag();
		}

		protected override async Task DiscardModuleProcess() {
			await Internal.DeleteModuleTag();
		}
	}
}