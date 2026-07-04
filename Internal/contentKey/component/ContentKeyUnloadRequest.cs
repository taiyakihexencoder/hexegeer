using Unity.Entities;

namespace hexegeer.internallib {
	public struct ContentKeyUnloadRequest : IComponentData {
		public int contentKey;
	}
}