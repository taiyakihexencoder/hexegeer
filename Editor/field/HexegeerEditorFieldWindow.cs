using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerEditorFieldWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Field");
		}
	}
}