using System.IO;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	internal class HexegeerEditor {
		[MenuItem("Hexegeer/Setting Window")]
		private static void OpenSettingWindow() {
			HexegeerEditorGeneralWindow general = EditorWindow.CreateWindow<HexegeerEditorGeneralWindow>();
			System.Type windowType = general.GetType();
			EditorWindow.CreateWindow<HexegeerEditorCharacterWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorContentKeyWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorDamageObjectWindow>(windowType);
			EditorWindow.CreateWindow<HexegeerEditorEventPointWindow>(windowType);
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

		[MenuItem("Hexegeer/Master Data Window")]
		private static void OpenMasterDataWindow() {
			EditorWindow.CreateWindow<HexegeerMasterDataTop>();
		}

		[MenuItem("Hexegeer/Generate Addressable List")]
		private static void GenerateAddressableList() {
			try {
				AddressableListGenerator generator = new AddressableListGenerator();
				generator.Generate($"utility{Path.DirectorySeparatorChar}ResourceAddressList.cs");
			} catch (System.Exception e) {
				EditorUtility.DisplayDialog("Error", e.Message, "Ok");
				D.LogE(e);
			}
		}

		[MenuItem("Hexegeer/Generate Localize List")]
		private static void GenerateLocalizeList() {
			try {
				LocalizeListGenerator generator = new LocalizeListGenerator();
				generator.Generate($"utility{Path.DirectorySeparatorChar}LocalizeKey.cs");
			} catch (System.Exception e) {
				EditorUtility.DisplayDialog("Error", e.Message, "Ok");
				D.LogE(e);
			}
		}
	}
}