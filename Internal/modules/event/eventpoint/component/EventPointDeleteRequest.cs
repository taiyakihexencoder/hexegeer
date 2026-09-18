using Unity.Entities;

namespace hexegeer.internallib {
	public struct EventPointDeleteRequest : IComponentData {
		public int contentKey;
	}
}