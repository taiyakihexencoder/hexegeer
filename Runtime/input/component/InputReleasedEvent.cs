using Unity.Entities;

namespace hexegeer {
	public struct InputReleasedEvent : IBufferElementData {
		public InputButtonEventKey key;
	}
}