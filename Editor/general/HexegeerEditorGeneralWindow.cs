using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerEditorGeneralWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("General");
		}
	}
}
