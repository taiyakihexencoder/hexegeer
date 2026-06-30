using Unity.Entities;

namespace hexegeer {
	public struct SequenceRequest : IComponentData {
		public int contextKey;
		public int sequenceId;
	}
}