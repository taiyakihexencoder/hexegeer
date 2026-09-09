using Unity.GraphToolkit.Editor;

namespace hexegeer.editor {
	public interface IGraphToolkitSequenceNode { }

	public static class SequenceNodeExtension {
		public static void CreateOption(this IGraphToolkitSequenceNode node, Node.IOptionDefinitionContext context) {
			context.AddOption<int>("Port Count")
				.WithDefaultValue(0)
				.Build();
		}

		public static void CreateOutputPort<T>(this T node, Node.IPortDefinitionContext context)
			where T : Node, IGraphToolkitSequenceNode {
			if (node.GetNodeOptionByName("Port Count").TryGetValue(out int count)) {
				for (int i = 0; i < count; ++i) {
					context.AddOutputPort($"Sequence {i}").Build();
				}
			}
		}
	}
}