using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/DamageObject/DamageObjectSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class DamageObjectSettings : ScriptableSingleton<DamageObjectSettings> {
		[System.Serializable]
		public struct DamageObjectData {
			public int id;
			public string name;
			public List<int> contentKeys;
			public int collider;
		}

		[SerializeField]
		private List<DamageObjectData> _rows;
		public List<DamageObjectData> Rows {
			get {
				if (_rows == null) { _rows = new List<DamageObjectData>(); }
				return _rows;
			}
		}

		public void UpdateRow(int index, DamageObjectData row) {
			_rows[index] = row;
			Save(true);
		}

		public void AddRow() {
			List<int> ids = _rows.ConvertAll(_ => _.id);
			ids.Sort();
			int newId = 1;
			foreach(int id in ids) {
				if (newId < id) {
					break;
				}
				newId = id + 1;
			}

			_rows.Add(
				new DamageObjectData {
					id = newId,
					name = "newDamageObject",
					contentKeys = new List<int>(),
					collider = 0,
				}
			);
			Save(true);
		}

		public void RemoveRow(int index) {
			_rows.RemoveAt(index);
			Save(true);
		}

		public void MoveUpRow(int index) {
			DamageObjectData data = _rows[index];
			_rows[index] = _rows[index-1];
			_rows[index-1] = data;
			Save(true);
		}

		public void MoveDownRow(int index) {
			DamageObjectData data = _rows[index];
			_rows[index] = _rows[index+1];
			_rows[index+1] = data;
			Save(true);
		}
	}
}