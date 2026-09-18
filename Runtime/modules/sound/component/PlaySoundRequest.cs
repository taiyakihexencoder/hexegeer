using Unity.Entities;

namespace hexegeer {
	public struct PlaySoundRequest : IComponentData {
		public int id;
		public SoundType type;
	}
}