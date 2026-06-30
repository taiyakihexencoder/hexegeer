using System.Collections.Generic;
using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public sealed class SequencerContext {
		private static Dictionary<int, SequencerContext> _contexts;
		static SequencerContext() {
			_contexts = new Dictionary<int, SequencerContext>();
		}
		internal static Dictionary<int, SequencerContext> Contexts => _contexts;

		private Dictionary<int, SequenceNode> _nodes;

		private SequenceNode[] _currentTree;
		private int _currentNode;

		private bool _nowTranslate;

		private int _key = -1;

		internal SequencerContext(System.Action<SequenceNode> createTree) {
			_nodes = new Dictionary<int, SequenceNode>();
			_nowTranslate = false;

			SequenceNode root = new SequenceNode(new EmptyLayeredSequencer(-1));
			createTree(root);

			List<SequenceNode> openList = new List<SequenceNode>(){ root, };
			while(openList.Count > 0) {
				SequenceNode node = openList[0];
				openList.RemoveAt(0);
				_nodes.Add(node.sequencer.SequenceId, node);

				for (int i = 0; i < node.childCount; ++i) {
					SequenceNode child = node.GetChild(i);
					openList.Add(child);
				}
			}
			_currentNode = root.sequencer.SequenceId;
			_currentTree = new SequenceNode[0];

		}

		public static void Create(int key, System.Action<SequenceNode> createTree) {
			SequencerContext context = new SequencerContext(createTree);
			context.RegisterKey(key);
		}

		/// <summary>
		/// コンテキストを更新リストに登録
		/// </summary>
		/// <param name="key"></param>
		public void RegisterKey(int key) {
			_key = key;
			_contexts.Add(key, this);
		}

		/// <summary>
		/// コンテキストを更新リストから排除
		/// </summary>
		public void Unregister() {
			_contexts.Remove(_key);
		}

		/// <summary>
		/// コンテキストを更新リストから排除
		/// </summary>
		public static void Unregister(int key) {
			_contexts.Remove(key);
		}

		internal void OnUpdate() {
			if (! _nowTranslate) {
				foreach(SequenceNode node in _currentTree) {
					if (node != null) {
						node.sequencer.OnUpdate(this);
					} else break;
				}
			}
		}

		/// <summary>
		/// シーケンス移動依頼
		/// ILayeredSequence.OnUpdateで使う想定。
		/// </summary>
		/// <param name="sequenceId"></param>
		public void RequestSequence(int sequenceId) {
			SyncContext.Post(() => {
				HexegeerUtility.ECS.CreateEntity(new SequenceRequest{
					contextKey = _key,
					sequenceId = sequenceId,
				})
			});
		}

		internal void ChangeSequence(int sequenceId) {
			if (_nowTranslate) {
				D.Log($"Translation locked: ignore={sequenceId}");
				return;
			}

			if (_nodes.TryGetValue(_currentNode, out SequenceNode from) && _nodes.TryGetValue(sequenceId, out SequenceNode to)) {
				List<int> currentList = new List<int>();
				SequenceNode n = from;
				while(n.parent != null) {
					currentList.Insert(0, n.sequencer.SequenceId);
					n = n.parent;
				}

				List<int> nextList = new List<int>();
				n = to;
				while(n.parent != null) {
					nextList.Insert(0, n.sequencer.SequenceId);
					n = n.parent;
				}

				int matchedIndex = 0;
				int compareLength = math.min(nextList.Count, currentList.Count);
				while(matchedIndex < compareLength) {
					if (currentList[matchedIndex] != nextList[matchedIndex]) {
						break;
					}
					matchedIndex++;
				}

				_nowTranslate = true;
				Task.Run(() => Transition(currentList.ToArray(), nextList.ToArray(), matchedIndex));
			}
		}

		private async Task Transition(int[] upIndices, int[] downIndices, int rootIndex) {
			// 共通しているレイヤーまで上っていく
			for(int i = upIndices.Length-1; i >= rootIndex; ++i) {
				_currentNode = upIndices[i];
				if (_nodes.TryGetValue(upIndices[i], out SequenceNode node)) {
					await node.sequencer.OnExit(this);
				} else break;
			}

			// 共通しているレイヤーから目的のレイヤーまで下りていく
			for (int i = rootIndex; i < downIndices.Length; ++i) {
				_currentNode = downIndices[i];
				if (_nodes.TryGetValue(downIndices[i], out SequenceNode node)) {
					await node.sequencer.OnEnter(this);
				} else break;
			}

			// ツリー情報の更新
			_currentTree = new SequenceNode[downIndices.Length];
			for (int i = 0; i < downIndices.Length; ++i) {
				if (_nodes.TryGetValue(downIndices[i], out SequenceNode node)) {
					_currentTree[i] = node;
				} else break;
			}

			_nowTranslate = false;
		}
	}
}