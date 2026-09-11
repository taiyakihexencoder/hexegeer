using Unity.GraphToolkit.Editor;

namespace hexegeer.editor {
	public static class INodeExtension {
		public static bool TryGetInput<T>(this INode node, string name, out T value) {
			IPort port = node.GetInputPortByName(name);
			if (port == null) {
				value = default;
				return false;
			} else if (port.TryGetValue(out value)) {
				return true;
			} else if (port.FirstConnectedPort.GetNode() is IConstantNode constantNode) {
				if (constantNode.TryGetValue(out value)) {
					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

		public static bool TryGetOption<T>(this INode node, string name, out T value) {
			INodeOption option = node.GetNodeOptionByName(name);
			if (option == null) {
				value = default;
				return false;
			} else {
				return option.TryGetValue(out value);
			}
		}
	}
}