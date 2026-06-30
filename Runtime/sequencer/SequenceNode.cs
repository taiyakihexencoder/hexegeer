using System.Collections.Generic;

namespace hexegeer {
	public sealed class SequenceNode {
		private SequenceNode _parent;
		public SequenceNode parent => _parent;

		private List<SequenceNode> _children;
		public int childCount => _children.Count;

		private ILayeredSequencer _sequencer;
		public ILayeredSequencer sequencer => _sequencer;

		public static SequenceNode Create(SequenceNode parent, ILayeredSequencer sequencer) => new SequenceNode(parent, sequencer);
		public static SequenceNode Create(ILayeredSequencer sequencer) => new SequenceNode(sequencer);

		public SequenceNode(SequenceNode parent, ILayeredSequencer sequencer): this(sequencer) {
			SetParent(parent);
		}

		public SequenceNode(ILayeredSequencer sequencer) {
			_parent = null;
			_sequencer = sequencer;
			_children = new List<SequenceNode>();
		}

		public SequenceNode GetChild(int index) {
			return _children[index];
		}

		private void AddChild(SequenceNode node) {
			_children.Add(node);
		}

		private void SetParent(SequenceNode parent) {
			_parent = parent;
			parent?.AddChild(this);
		}

		public SequenceNode WithChildren(params SequenceNode[] children) {
			foreach(SequenceNode child in children) {
				child.SetParent(this);
			}
			return this;
		}
	}
}