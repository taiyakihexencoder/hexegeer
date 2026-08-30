using System.IO;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerMasterDataTable : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Table Sample");

			EditorGridView grid = new EditorGridView(
				"StreamingAssets" + Path.DirectorySeparatorChar + "sample.bytes",
				new EditorGridView.Column(1, "Column 1", EditorGridView.ColumnType.INT),
				new EditorGridView.Column(2, "Column 2", EditorGridView.ColumnType.BOOL),
				new EditorGridView.Column(3, "Column 3", EditorGridView.ColumnType.FLOAT),
				new EditorGridView.Column(4, "Column 4", EditorGridView.ColumnType.STRING),
				new EditorGridView.Column(5, "Column 5", EditorGridView.ColumnType.INT),
				new EditorGridView.Column(6, "Column 6", EditorGridView.ColumnType.INT),
				new EditorGridView.Column(7, "Column 7", EditorGridView.ColumnType.INT),
				new EditorGridView.Column(8, "Column 8", EditorGridView.ColumnType.INT),
				new EditorGridView.Column(9, "Column 9", EditorGridView.ColumnType.INT),
				new EditorGridView.Column(10, "Column 10", EditorGridView.ColumnType.INT)
			);

			rootVisualElement.Add(grid);
		}
	}
}