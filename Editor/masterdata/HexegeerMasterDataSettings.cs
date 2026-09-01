using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/MasterData/MasterDataSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class HexegeerMasterDataSettings : ScriptableSingleton<HexegeerMasterDataSettings> {
		
		[System.Serializable]
		public sealed class DataClass {
			public int id;
			public string className;
			public string fileName;
			public List<EditorGridView.Column> columns;
		}

		[SerializeField]
		private List<DataClass> _classList;
		public List<DataClass> ClassList {
			get {
				if (_classList == null) { _classList = new List<DataClass>(); }
				return _classList;
			}
		}

		public void AddClass() {
			List<int> ids = ClassList.ConvertAll(_ => _.id);
			ids.Sort();
			int newId = 1;
			foreach(int id in ids) {
				if (newId < id) { break; }
				newId = id + 1;
			}

			ClassList.Add(
				new DataClass {
					id = newId,
					className = "new class",
					fileName = "newclass.bytes",
					columns = new List<EditorGridView.Column>(),
				}
			);
			instance.Save(true);
		}

		public void RemoveClass(int index) {
			ClassList.RemoveAt(index);
			instance.Save(true);
		}

		public void MoveUpClass(int index) {
			DataClass classData = ClassList[index];
			ClassList[index] = ClassList[index-1];
			ClassList[index-1] = classData;
			instance.Save(true);
		}

		public void MoveDownClass(int index) {
			DataClass classData = ClassList[index];
			ClassList[index] = ClassList[index+1];
			ClassList[index+1] = classData;
			instance.Save(true);
		}

		public void UpdateClassName(int index, string className) {
			ClassList[index].className = className;
			Save(true);
		}

		public void UpdateFileName(int index, string fileName) {
			ClassList[index].fileName = fileName;
			Save(true);
		}

		public void AddColumn(int classIndex) {
			List<int> ids = ClassList[classIndex].columns.ConvertAll(_ => _.Id);
			int newId = 1;
			foreach(int id in ids) {
				if (newId < id) { break; }
				newId = id + 1;
			}

			ClassList[classIndex].columns.Add(
				new EditorGridView.Column(newId, "new columns", EditorGridView.ColumnType.INT)
			);
			instance.Save(true);
		}

		public void RemoveColumn(int classIndex, int columnIndex) {
			ClassList[classIndex].columns.RemoveAt(columnIndex);
			Save(true);
		}

		public void MoveUpColumn(int classIndex, int columnIndex) {
			EditorGridView.Column column = ClassList[classIndex].columns[columnIndex];
			ClassList[classIndex].columns[columnIndex] = ClassList[classIndex].columns[columnIndex-1];
			ClassList[classIndex].columns[columnIndex-1] = column;
			Save(true);
		}

		public void MoveDownColumn(int classIndex, int columnIndex) {
			EditorGridView.Column column = ClassList[classIndex].columns[columnIndex];
			ClassList[classIndex].columns[columnIndex] = ClassList[classIndex].columns[columnIndex+1];
			ClassList[classIndex].columns[columnIndex+1] = column;
			Save(true);
		}

		public void UpdateColumn(int classIndex, int columnIndex, EditorGridView.Column column) {
			ClassList[classIndex].columns[columnIndex] = column;
			Save(true);
		}
	}
}