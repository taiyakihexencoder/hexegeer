using Unity.Entities;

namespace hexegeer {
	public struct InputPressedEvent : IBufferElementData {
		public InputButtonEventKey key;
	}
}