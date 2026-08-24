using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/EventPoint/EventPointSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class EventPointSettings : ScriptableSingleton<EventPointSettings> {
		[System.Serializable]
		public class EventInfo {
			public int eventId;
			public string name;
			public string description;
		}

		[SerializeField]
		private List<EventInfo> _rows;
		public List<EventInfo> Rows {
			get {
				if (_rows == null) { _rows = new List<EventInfo>(); }
				return _rows;
			}
		}

		public void UpdateParameter(int index, in EventInfo info) {
			_rows[index] = info;
			Save(true);
		}

		public void MoveUp(int index) {
			EventInfo temp = _rows[index];
			_rows[index] = _rows[index-1];
			_rows[index-1] = temp;
			Save(true);
		}

		public void MoveDown(int index) {
			EventInfo temp = _rows[index];
			_rows[index] = _rows[index+1];
			_rows[index+1] = temp;
			Save(true);
		}

		public void RemoveAt(int index) {
			_rows.RemoveAt(index);
			Save(true);
		}

		public void Add() {
			int newId = 1;
			List<int> ids = _rows.ConvertAll(_ => _.eventId);
			ids.Sort();
			foreach(int id in ids) {
				if (newId < id) {
					break;
				}
				newId = id + 1;
			}

			_rows.Add(
				new EventInfo {
					eventId = newId,
					name = "new event",
				}
			);
			Save(true);
		}

		public ListPopupBuilder<int> CreateListPopupBuilder() {
			ListPopupBuilder<int> builder = new ListPopupBuilder<int>();

			builder.SetConverter(key => {
				foreach(EventInfo row in Rows) {
					if (row.eventId == key) { return row.name; }
				}
				return " - ";
			});

			builder = UpdateKeys(builder);
			return builder;
		}

		public ListPopupBuilder<int> UpdateKeys(ListPopupBuilder<int> builder) {
			List<int> keys = new List<int>();
			foreach(EventInfo row in Rows) {
				keys.Add(row.eventId);
			}
			return builder.SetKeys(keys);
		}
	}
}