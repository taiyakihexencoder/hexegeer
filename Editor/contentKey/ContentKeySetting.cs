using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/ContentKey/ContentKeySetting.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class ContentKeySetting : ScriptableSingleton<ContentKeySetting> {
		[System.Serializable]
		public class Key {
			public int id;
			public string name;
		}

		[SerializeField]
		private List<Key> _keys;
		public List<Key> Keys => _keys;

		public void SetName(Key key, string name) {
			for (int i = 0; i < _keys.Count; ++i) {
				if (key.id == _keys[i].id) {
					_keys[i].name = name;
					Save(true);
					break;
				}
			}
		}

		public void Add(string name) {
			int id = 1;

			List<int> ids = new List<int>();
			for(int i = 0; i < _keys.Count; ++i) {
				ids.Add(_keys[i].id);
			}
			ids.Sort();
			for(int i = 0; i < ids.Count; ++i) {
				if (ids[i] - id > 0) {
					break;
				}
				id = ids[i]+1;
			}

			_keys.Add(
				new Key { id = id, name = name, }
			);
			Save(true);
		}

		public void Remove(int id) {
			_keys.RemoveAll(_ => _.id == id);
			Save(true);
		}

		public ListPopupBuilder<int> CreateListPopupBuilder() {
			ListPopupBuilder<int> builder = new ListPopupBuilder<int>();

			builder.SetConverter(key => {
				foreach (Key k in instance.Keys) {
					if (k.id == key) { return k.name; }
				}
				if (key == 0) { return "Global"; }

				return " - ";
			});

			builder = UpdateKeys(builder);
			return builder;
		}

		public ListPopupBuilder<int> UpdateKeys(ListPopupBuilder<int> builder) {
			List<int> keys = new List<int>(){0};
			foreach(Key k in Keys) {
				keys.Add(k.id);
			}

			return builder.SetKeys(keys);
		}
	}
}