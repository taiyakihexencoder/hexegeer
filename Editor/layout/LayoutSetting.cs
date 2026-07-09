using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Layout/LayoutSetting.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class LayoutSetting : ScriptableSingleton<LayoutSetting> {
		[System.Serializable]
		public class LayoutProfile {
			public int contentKey;
			public List<CharacterLayout> characters;
		}

		[System.Serializable]
		public class CharacterLayout {
			public int character;
			public Vector3 position;
			public Quaternion rotation;
		}

		[SerializeField]
		private List<LayoutProfile> _layoutProfiles;
		public List<LayoutProfile> LayoutProfiles {
			get { return _layoutProfiles; }
			set { 
				_layoutProfiles = value; 
				Save(true);
			}
		}

		public void UpdateCharacter(
			int layout,
			int index,
			int characterKey
		) {
			_layoutProfiles[layout].characters[index].character = characterKey;
			Save(true);
		}

		public void UpdateCharacterPosition(
			int layout,
			int index, 
			Vector3 position
		) {
			_layoutProfiles[layout].characters[index].position = position;
			Save(true);
		}

		public void UpdateCharacterRotation(
			int layout,
			int index, 
			Quaternion rotation
		) {
			_layoutProfiles[layout].characters[index].rotation = rotation;
			Save(true);
		}

		public void AddCharacter(int layout) {
			_layoutProfiles[layout].characters.Add(
				new CharacterLayout {
					character = 0,
					position = Vector3.zero,
					rotation = Quaternion.identity,
				}
			);
			Save(true);
		}

		public void RemoveCharacter(int layout, int index) {
			_layoutProfiles[layout].characters.RemoveAt(index);
		}

		public void UpdateLayouts(List<int> keys) {
			List<LayoutProfile> layouts = new List<LayoutProfile>();
			foreach (int key in keys) {
				int index = _layoutProfiles.FindIndex(_ => _.contentKey == key);
				if (index >= 0) {
					layouts.Add(_layoutProfiles[index]);
					_layoutProfiles.RemoveAt(index);
				} else {
					layouts.Add(new LayoutProfile{
						characters = new List<CharacterLayout>(),
						contentKey = key,
					});
				}
			}
			_layoutProfiles = layouts;
			Save(true);
		}
	}
}