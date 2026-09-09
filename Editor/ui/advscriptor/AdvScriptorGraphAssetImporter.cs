using System.Collections.Generic;
using hexegeer.internallib;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace hexegeer.editor {
	[ScriptedImporter(1, AdvScriptorGraph.Ext)]
	internal class AdvScriptorGraphAssetImporter : ScriptedImporter {
		public override void OnImportAsset(AssetImportContext ctx) {
			AdvScriptorGraph graph = GraphDatabase.LoadGraphForImporter<AdvScriptorGraph>(ctx.assetPath);

			if (graph == null) {
				D.LogE($"Fail load graph: {ctx.assetPath}");
				return;
			}

			AdvStart startNode = null;
			foreach(INode node in graph.GetNodes()) {
				if (node is AdvStart start) {
					startNode = start;
					break;
				}
			}

			if (startNode == null) {
				D.LogE($"Start node not found.");
			}

			int currentIndex = -1;

			AdvScript script = ScriptableObject.CreateInstance<AdvScript>();
			SerializedObject serializedObject = new SerializedObject(script);

			OpenNode(ref currentIndex, -1, serializedObject, startNode, 0, 1);

			serializedObject.ApplyModifiedProperties();

			ctx.AddObjectToAsset("main", script);
			ctx.SetMainObject(script);
		}

		private void OpenNode(ref int currentIndex, int parentIndex, SerializedObject serializedObject, INode node, int listIndex, int listCount) {
			List<IPort> connectedPort = new List<IPort>();
			int seqIndex = currentIndex;
			currentIndex++;
			AddNode(serializedObject, node, seqIndex, parentIndex, listIndex, listCount);

			List<INode> nodes = new List<INode>();
			foreach(IPort outPort in node.GetOutputPorts()) {
				outPort.GetConnectedPorts(connectedPort);
				for (int i = 0; i < connectedPort.Count; ++i) {
					nodes.Add(connectedPort[i].GetNode());
				}
			}

			for(int i = 0; i < nodes.Count; ++i) {
				OpenNode(ref currentIndex, seqIndex, serializedObject, nodes[i], i, nodes.Count);
			}
		}

		private void AddNode(SerializedObject serializedObject, INode node, int seqIndex, int parentIndex, int listIndex, int listCount) {
			if (node is AdvStart nodeStart) {
				// Nothing to do
			} else if (node is AdvSequencer nodeSequencer) {
				AddNodeSequencer(serializedObject, nodeSequencer, seqIndex, parentIndex);
			} else if (node is AdvWindowSequence nodeWindowSequence) {
				AddNodeWindowSequence(serializedObject, nodeWindowSequence, seqIndex, parentIndex);
			} else if (node is AdvText nodeText) {
				int nextIndex = listIndex == listCount-1 ? parentIndex : seqIndex+1;
				AddNodeText(serializedObject, nodeText, seqIndex, nextIndex);
			}
		}

		private void AddNodeSequencer(SerializedObject serializedObject, AdvSequencer node, int seqIndex, int parentIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_sequenceProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_parentIndex").intValue = parentIndex;
				INodeOption countOption = node.GetNodeOptionByName("Port Count");
				if (countOption.TryGetValue(out int count)) {
					property.FindPropertyRelative("_count").intValue = count;
				}
			});
		}

		private void AddNodeWindowSequence(SerializedObject serializedObject, AdvWindowSequence node, int seqIndex, int parentIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_windowSequenceProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_parentIndex").intValue = parentIndex;
				INodeOption countOption = node.GetNodeOptionByName("Port Count");
				if (countOption.TryGetValue(out int count)) {
					property.FindPropertyRelative("_count").intValue = count;
				}
			});
		}

		private void AddNodeText(SerializedObject serializedObject, AdvText node, int seqIndex, int nextIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_playTextProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_nextIndex").intValue = nextIndex;
				INodeOption speakerOption = node.GetNodeOptionByName("Speaker");
				if (speakerOption.TryGetValue(out LocalizeKey speaker)) {
					property.FindPropertyRelative("_speaker").stringValue = speaker.Name;
				}

				INodeOption textOption = node.GetNodeOptionByName("Text");
				if (textOption.TryGetValue(out LocalizeKey text)) {
					property.FindPropertyRelative("_text").stringValue = text.Name;
				}
			});
		}
	}
}