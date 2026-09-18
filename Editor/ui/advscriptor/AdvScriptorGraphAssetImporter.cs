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

			List<INode> nodes = new List<INode>();
			foreach(IPort outPort in node.GetOutputPorts()) {
				outPort.GetConnectedPorts(connectedPort);
				for (int i = 0; i < connectedPort.Count; ++i) {
					nodes.Add(connectedPort[i].GetNode());
				}
			}

			int[] children = new int[nodes.Count];
			for(int i = 0; i < nodes.Count; ++i) {
				children[i] = currentIndex;
				OpenNode(ref currentIndex, seqIndex, serializedObject, nodes[i], i, nodes.Count);
			}
			AddNode(serializedObject, node, seqIndex, parentIndex, children, listIndex, listCount);
		}

		private void AddNode(SerializedObject serializedObject, INode node, int seqIndex, int parentIndex, in int[] childrenIndices, int listIndex, int listCount) {
			if (node is AdvStart nodeStart) {
				// Nothing to do
			} else if (node is AdvSequencer nodeSequencer) {
				AddNodeSequencer(serializedObject, nodeSequencer, seqIndex, parentIndex, childrenIndices);
			} else if (node is AdvWindowSequence nodeWindowSequence) {
				AddNodeWindowSequence(serializedObject, nodeWindowSequence, seqIndex, parentIndex);
			} else if (node is AdvText nodeText) {
				int nextIndex = listIndex == listCount-1 ? parentIndex : seqIndex+1;
				AddNodeText(serializedObject, nodeText, seqIndex, nextIndex);
			} else if (node is AdvWaitInput waitInput) {
				int nextIndex = listIndex == listCount-1 ? parentIndex : seqIndex+1;
				AddNodeWaitInput(serializedObject, waitInput, seqIndex, nextIndex);
			} else if (node is AdvWaitSeconds waitSeconds) {
				int nextIndex = listIndex == listCount-1 ? parentIndex : seqIndex+1;
				AddNodeWaitSeconds(serializedObject, waitSeconds, seqIndex, nextIndex);
			} else if (node is AdvEnd end) {
				AddNodeEnd(serializedObject, end, seqIndex);
			}
		}

		private void AddNodeSequencer(SerializedObject serializedObject, AdvSequencer node, int seqIndex, int parentIndex, int[] childrenIndices) {
			SerializedProperty properties = serializedObject.FindProperty("_sequencerProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_parentIndex").intValue = parentIndex;

				SerializedProperty childrenIndicesProperty = property.FindPropertyRelative("_childIndices");
				childrenIndicesProperty.arraySize = childrenIndices.Length;
				for(int i = 0; i < childrenIndices.Length; ++i) {
					childrenIndicesProperty.Of(i).intValue = childrenIndices[i];
				}
			});
		}

		private void AddNodeWindowSequence(SerializedObject serializedObject, AdvWindowSequence node, int seqIndex, int parentIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_windowSequenceProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_parentIndex").intValue = parentIndex;

				if (node.TryGetOption("Port Count", out int count)) {
					property.FindPropertyRelative("_count").intValue = count;
				}
			});
		}

		private void AddNodeText(SerializedObject serializedObject, AdvText node, int seqIndex, int nextIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_playTextProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_nextIndex").intValue = nextIndex;

				if (node.TryGetOption("Speaker", out LocalizeKey speaker)) {
					property.FindPropertyRelative("_speaker").stringValue = speaker.Name;
				}

				if (node.TryGetOption("Text", out LocalizeKey text)) {
					property.FindPropertyRelative("_text").stringValue = text.Name;
				}
			});
		}

		private void AddNodeWaitInput(SerializedObject serializedObject, AdvWaitInput node, int seqIndex, int nextIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_waitInputProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_nextIndex").intValue = nextIndex;
			});
		}

		private void AddNodeWaitSeconds(SerializedObject serializedObject, AdvWaitSeconds node, int seqIndex, int nextIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_waitSecondsProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				property.FindPropertyRelative("_nextIndex").intValue = nextIndex;
				if (node.TryGetInput("Seconds", out float value)) {
					property.FindPropertyRelative("_seconds").floatValue = value;
				}
			});
		}

		private void AddNodeEnd(SerializedObject serializedObject, AdvEnd node, int seqIndex) {
			SerializedProperty properties = serializedObject.FindProperty("_endProcess");
			properties.Add(property => {
				property.FindPropertyRelative("_sequenceIndex").intValue = seqIndex;
				if (node.TryGetOption("Type", out AdvEndType endType)) {
					property.FindPropertyRelative("_typeId").intValue = endType.Id;
				}
			});
		}
	}
}