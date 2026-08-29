using Unity.Entities;

namespace hexegeer {
	public struct EventElement : IBufferElementData {
		public int eventId;
		public Entity entity;
	}
}