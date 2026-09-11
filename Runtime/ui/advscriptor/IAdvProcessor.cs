namespace hexegeer {
	public interface IAdvProcessor {
		void ProcessWindowSequence(bool end, System.Action setFlag);

		void ProcessPlayText(string speaker, string text, System.Action setFlag);

		void ProcessWaitInput(System.Action setFlag);
	}
}