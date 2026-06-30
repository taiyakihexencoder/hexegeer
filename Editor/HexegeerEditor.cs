using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	internal class HexegeerEditor {
		[MenuItem("Hexegeer/Setting Window")]
		private static void Open() {
			HexegeerEditorGeneralWindow general = EditorWindow.CreateWindow<HexegeerEditorGeneralWindow>();
			System.Type windowType = general.GetType();
			HexegeerEditorCharacterWindow character = EditorWindow.CreateWindow<HexegeerEditorCharacterWindow>(windowType);
			HexegeerEditorFieldWindow field = EditorWindow.CreateWindow<HexegeerEditorFieldWindow>(windowType);
			HexegeerEditorLayerWindow layer = EditorWindow.CreateWindow<HexegeerEditorLayerWindow>(windowType);
			HexegeerEditorSaveWindow save = EditorWindow.CreateWindow<HexegeerEditorSaveWindow>(windowType);
			general.Focus();
		}
	}
}