using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerEditorCharacterWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Character");
		}
	}
}
