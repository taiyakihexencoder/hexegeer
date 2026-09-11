using UnityEngine;

namespace hexegeer.internallib {
	public sealed class AdvScript : ScriptableObject {
		[System.Serializable]
		public abstract class AdvProcessData {
			[SerializeField]
			private int _sequenceIndex;
			public int SequenceIndex => _sequenceIndex;
		}

		[System.Serializable]
		public abstract class OneShotAdvProcessData : AdvProcessData {
			[SerializeField]
			private int _nextIndex;
			public int NextIndex => _nextIndex;
		}

		[System.Serializable]
		public class SequencerProcessData : AdvProcessData {
			[SerializeField]
			private int _parentIndex;
			public int ParentIndex => _parentIndex;

			[SerializeField]
			private int[] _childIndices;
			public int[] ChildIndices => _childIndices;
			public int Count => _childIndices.Length;

			private int _currentChildIndex;

			public void ResetCurrentChild() {
				_currentChildIndex = 0;
			}

			public int Next() {
				int index = _currentChildIndex;
				if (_currentChildIndex >= _childIndices.Length) {
					return _parentIndex;
				} else {
					_currentChildIndex++;
					return _childIndices[index];
				}
			}
		}

		[System.Serializable]
		public class WindowSequenceProcessData : AdvProcessData {
			[SerializeField]
			private int _parentIndex;
			public int ParentIndex => _parentIndex;

			public int StartIndex => SequenceIndex + 1;

			[SerializeField]
			private int _count;
			public int Count => _count;

			private bool _calledOnce = false;
			public bool CalledOnce => _calledOnce;

			public void SetCalled(bool called) {
				_calledOnce = called;
			}
		}

		[System.Serializable]
		public class PlayTextProcessData : OneShotAdvProcessData {
			[SerializeField]
			private string _speaker;
			public string Speaker => _speaker;

			[SerializeField]
			private string _text;
			public string Text => _text;
		}

		[System.Serializable]
		public class WaitInputProcessData : OneShotAdvProcessData { }

		[System.Serializable]
		public class WaitSecondsProcessData : OneShotAdvProcessData { 
			[SerializeField]
			private float _seconds;
			public float Seconds => _seconds;
		}

		[SerializeField]
		private SequencerProcessData[] _sequencerProcess;
		public SequencerProcessData[] SequencerProcess => _sequencerProcess;

		[SerializeField]
		private WindowSequenceProcessData[] _windowSequenceProcess;
		public WindowSequenceProcessData[] WindowSequenceProcess => _windowSequenceProcess;

		[SerializeField]
		private PlayTextProcessData[] _playTextProcess;
		public PlayTextProcessData[] PlayTextProcess => _playTextProcess;

		[SerializeField]
		private WaitInputProcessData[] _waitInputProcess;
		public WaitInputProcessData[] WaitInputProcess => _waitInputProcess;

		[SerializeField]
		private WaitSecondsProcessData[] _waitSecondsProcess;
		public WaitSecondsProcessData[] WaitSecondsProcess => _waitSecondsProcess;
	}
}