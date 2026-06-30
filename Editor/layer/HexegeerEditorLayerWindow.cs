using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerEditorLayerWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Layer");
		}
	}
}
