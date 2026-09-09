using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[Graph(AdvScriptorGraph.Ext)]
	[System.Serializable]
	public sealed class AdvScriptorGraph : Graph {
		public const string Ext = "geeradvg";

		[MenuItem("Assets/Create/Hexegeer/Adv Scriptor")]
		private static void CreateAsset() {
			GraphDatabase.PromptInProjectBrowserToCreateNewAsset<AdvScriptorGraph>();
		}

		public override void OnEnable(){
			base.OnEnable();
			foreach(INode node in GetNodes()) {
				if (node is AdvStart) { return; }
			}

			EditorApplication.delayCall += () => {
				AdvStart startNode = new AdvStart();
				startNode.Position = new Vector2(0f, 0f);
				AddNode(startNode);
			};
		}

		public override void OnGraphChanged(GraphLogger graphLogger) {
			int startNodeCount = 0;
			foreach(INode node in GetNodes()) {
				if (node is AdvStart) {
					startNodeCount++;
					if (startNodeCount > 1) {
						graphLogger.LogError($"{typeof(AdvStart).Name}", node);
					}
				}
			}
		}
	}

	[System.Serializable, UseWithGraph(typeof(AdvScriptorGraph))]
	public sealed class AdvStart : Node, IGraphToolkitSequenceNode {
		protected override void OnDefinePorts(IPortDefinitionContext context) {
			context.AddOutputPort("");
		}
	}

	[System.Serializable, UseWithGraph(typeof(AdvScriptorGraph))]
	public sealed class AdvSequencer : Node, IGraphToolkitSequenceNode {
		protected override void OnDefineOptions(IOptionDefinitionContext context) {
			this.CreateOption(context);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context) {
			context.AddInputPort("");
			this.CreateOutputPort(context);
		}
	}

	[System.Serializable, UseWithGraph(typeof(AdvScriptorGraph))]
	public sealed class AdvWindowSequence : Node, IGraphToolkitSequenceNode {
		protected override void OnDefineOptions(IOptionDefinitionContext context) {
			this.CreateOption(context);
		}

		protected override void OnDefinePorts(IPortDefinitionContext context) {
			context.AddInputPort("");
			this.CreateOutputPort(context);
		}
	}

	[System.Serializable, UseWithGraph(typeof(AdvScriptorGraph))]
	public sealed class AdvText : Node, IGraphToolkitSequenceNode {
		protected override void OnDefineOptions(IOptionDefinitionContext context) {
			context.AddOption<LocalizeKey>("Speaker")
				.Build();

			context.AddOption<LocalizeKey>("Text")
				.Build();
		}

		protected override void OnDefinePorts(IPortDefinitionContext context) {
			context.AddInputPort("");
		}
	}
}