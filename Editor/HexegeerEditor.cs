using System.IO;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	internal class HexegeerEditor {
		[MenuItem("Hexegeer/Setting Window")]
		private static void Open() {
			HexegeerEditorGeneralWindow general = EditorWindow.CreateWindow<HexegeerEditorGeneralWindow>();
			System.Type windowType = general.GetType();
			EditorWindow.CreateWindow<HexegeerEditorCharacterWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorContentKeyWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorDamageObjectWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorFieldWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorLayerWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorLayoutWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorSaveWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorSoundWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorVersionWindow>(windowType);
			general.Focus();
		}

		[MenuItem("Hexegeer/Paths/Open Persistent Data Folder")]
		private static void OpenPersistentDataFolder() {
			EditorUtility.RevealInFinder(Application.persistentDataPath + Path.DirectorySeparatorChar);
		}

		[MenuItem("Hexegeer/Paths/Open Streaming Asset Folder")]
		private static void OpenStreamingAssetFolder() {
			EditorUtility.RevealInFinder(Application.streamingAssetsPath + Path.DirectorySeparatorChar);
		}
	}
}