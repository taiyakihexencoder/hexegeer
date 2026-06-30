using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerEditorSaveWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Save");
		}
	}
}
