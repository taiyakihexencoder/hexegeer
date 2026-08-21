namespace hexegeer {
	public interface IPostProcessRunner {
		bool Unused { get; }

		void Setup();
		void Update(float elapsed);
	}
}