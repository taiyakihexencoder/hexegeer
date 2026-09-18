using Unity.Entities;

namespace hexegeer {
	public interface IAdvProcessor {

		void ProcessWindowSequence(bool end, System.Action setFlag);

		void ProcessPlayText(in InputMainStick stick, DynamicBuffer<InputReleasedEvent> evts, string speaker, string text, System.Action setFlag);

		void ProcessWaitInput(in InputMainStick stick, DynamicBuffer<InputReleasedEvent> evts, System.Action setFlag);
		void ProcessEnd(EntityCommandBuffer commandBuffer, int endType);
	}
}