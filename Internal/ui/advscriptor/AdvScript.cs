using UnityEngine;

namespace hexegeer.internallib {
	public sealed class AdvScript : ScriptableObject {
		[System.Serializable]
		public abstract class AdvProcessData {
			[SerializeField]
			private int _sequenceIndex;
			public int sequenceIndex => _sequenceIndex;
		}

		[System.Serializable]
		public abstract class OneShotAdvProcessData : AdvProcessData {
			[SerializeField]
			private int _nextIndex;
			public int nextIndex => _nextIndex;
		}

		[System.Serializable]
		public abstract class StartEndAdvProcessData : AdvProcessData {
			[SerializeField]
			private int _parentIndex;
			public int ParentIndex => _parentIndex;

			[SerializeField]
			private int _count;
			public int Count => _count;
		}

		[System.Serializable]
		public class SequenceProcessData : StartEndAdvProcessData { }

		[System.Serializable]
		public class WindowSequenceProcessData : StartEndAdvProcessData { }

		[System.Serializable]
		public class PlayTextProcessData : OneShotAdvProcessData {
			[SerializeField]
			private string _speaker;
			public string Speaker => _speaker;

			[SerializeField]
			private string _text;
			public string Text => _text;
		}

		[SerializeField]
		private SequenceProcessData[] _sequenceProcess;
		public SequenceProcessData[] SequencePricess => _sequenceProcess;

		[SerializeField]
		private WindowSequenceProcessData[] _windowSequenceProcess;
		public WindowSequenceProcessData[] WindowSequenceProcess => _windowSequenceProcess;

		[SerializeField]
		private PlayTextProcessData[] _playTextProcess;
		public PlayTextProcessData[] PlayTextProcess => _playTextProcess;
	}
}