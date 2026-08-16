using Unity.Entities;

namespace hexegeer.internallib {
	public struct EntityOwner : IComponentData {
		public Entity owner;
	}
}