using Unity.Entities;

namespace hexegeer.internallib {
	public struct EventPoint : IComponentData {
		public int contentKey;
		public int eventId;
	}
}