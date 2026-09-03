using System.IO;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerMasterDataTable : EditorWindow {
		public static string TablePath => $"StreamingAssets{Path.DirectorySeparatorChar}hexegeer{Path.DirectorySeparatorChar}data{Path.DirectorySeparatorChar}";
		private int _id;
		private EditorGridView _gridView = null;

		public static HexegeerMasterDataTable Open(int id) {
			HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
			HexegeerMasterDataSettings.DataClass data = settings.ClassList.Find(_ => _.id == id);

			System.Type type = typeof(HexegeerMasterDataTop);
			HexegeerMasterDataTable window = CreateWindow<HexegeerMasterDataTable>(type);

			window._id = id;
			if (data != null) {
				window.titleContent = new GUIContent(data.className);
			}
			return window;
		}

		private void OnFocus() {
			HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
			HexegeerMasterDataSettings.DataClass data = settings.ClassList.Find(_ => _.id == _id);
			if (data != null && _gridView == null) {
				_gridView = new EditorGridView($"{TablePath}{data.fileName}", data.columns.ToArray());
				rootVisualElement.Add(_gridView);
			}
		}

		private void OnLostFocus() {
			if (_gridView != null) {
				rootVisualElement.Remove(_gridView);
				_gridView = null;
			}
		}
	}
}