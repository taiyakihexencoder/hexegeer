using Unity.Entities;

namespace hexegeer.internallib {
	public struct EventElement : IBufferElementData {
		public int eventId;
		public Entity entity;
	}
}