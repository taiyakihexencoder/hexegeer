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
		private Key[] _keys;
		public Key[] Keys => _keys;

		public void SetName(Key key, string name) {
			for (int i = 0; i < _keys.Length; ++i) {
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
			for(int i = 0; i < _keys.Length; ++i) {
				ids.Add(_keys[i].id);
			}
			ids.Sort();
			for(int i = 0; i < ids.Count; ++i) {
				if (ids[i] - id > 0) {
					break;
				}
				id = ids[i]+1;
			}

			Key[] newKeys = new Key[_keys.Length +1];
			System.Array.Copy(_keys, newKeys, _keys.Length);
			newKeys[_keys.Length] = new Key {
				id = id,
				name = name,
			};

			_keys = newKeys;
			Save(true);
		}

		public void Remove(int id) {
			if (_keys.Length > 0) {
				Key[] newKey = new Key[_keys.Length-1];

				for (int i = 0, j = 0; i < _keys.Length; ++i) {
					if (_keys[i].id != id) {
						newKey[j] = _keys[i];
						++j;
					}
				}
				_keys = newKey;
				Save(true);
			}
		}

		public ListPopupBuilder<int> CreateListPopupBuilder() {
			ListPopupBuilder<int> builder = new ListPopupBuilder<int>();

			builder.SetConverter(key => {
				foreach (Key k in instance.Keys) {
					if (k.id == key) { return k.name; }
				}
				if (key == 0) { return "Global"; }

				return "";
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