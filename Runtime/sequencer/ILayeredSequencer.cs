using System.Threading.Tasks;

namespace hexegeer {
	public interface ILayeredSequencer {
		int SequenceId { get; }

		Task OnEnter(SequencerContext context);

		void OnUpdate(SequencerContext context);

		Task OnExit(SequencerContext context);
	}

	public sealed class EmptyLayeredSequencer : ILayeredSequencer {
		private int _sequenceId;
		int ILayeredSequencer.SequenceId => _sequenceId;

		public EmptyLayeredSequencer(int sequenceId) {
			_sequenceId = sequenceId;
		}

		async Task ILayeredSequencer.OnEnter(SequencerContext context) { await Task.Yield(); }
		async Task ILayeredSequencer.OnExit(SequencerContext context) { await Task.Yield(); }
		void ILayeredSequencer.OnUpdate(SequencerContext context) { }
	}
}