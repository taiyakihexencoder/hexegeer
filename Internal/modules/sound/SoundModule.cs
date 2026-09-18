using System.Threading.Tasks;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerRuntimeModuleSystemGroup))]
	public partial class HexegeerSoundModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerSoundModuleTag>();
		}
	}
	
	public struct HexegeerSoundModuleTag : IComponentData { }

	public class HexegeerSoundModuleInternal : HexegeerModuleInternal { 
		private MusicTable _musicTable;
		public MusicTable MusicTable => _musicTable;

		private EnvironmentSoundTable _environmentSoundTable;
		public EnvironmentSoundTable EnvironmentSoundTable => _environmentSoundTable;

		private SystemSoundTable _systemSoundTable;
		public SystemSoundTable SystemSoundTable => _systemSoundTable;


		private MusicPlayer _musicPlayer;
		public MusicPlayer MusicPlayer => _musicPlayer;

		private SoundPlayer _soundPlayer;
		public SoundPlayer SoundPlayer => _soundPlayer;

		public async Task Launch() {
			if (_musicTable == null) {
				_musicTable = await AssetUtil.RequestLoad<MusicTable>(MusicTable.RESOURCE_ADDRESS);
			}

			if (_environmentSoundTable == null) {
				_environmentSoundTable = await AssetUtil.RequestLoad<EnvironmentSoundTable>(EnvironmentSoundTable.RESOURCE_ADDRESS);
			}

			if (_systemSoundTable == null) {
				_systemSoundTable = await AssetUtil.RequestLoad<SystemSoundTable>(SystemSoundTable.RESOURCE_ADDRESS);
			}

			SyncContext.Send(() => {
				_musicPlayer = new MusicPlayer(_musicTable);
			});
		}

		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerSoundModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerSoundModuleTag>();
			if (_musicTable != null) {
				_musicPlayer.OnDestroy();
				_musicTable = null;
				AssetUtil.Release(MusicTable.RESOURCE_ADDRESS);
			}
		}
	}
}
