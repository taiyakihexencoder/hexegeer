using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Layout/LayoutSetting.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class LayoutSetting : ScriptableSingleton<LayoutSetting> {
		[System.Serializable]
		public class LayoutProfile {
			public int contentKey;
			public List<CharacterLayout> characters;
			public List<EventLayout> events;
		}

		[System.Serializable]
		public class CharacterLayout {
			public int character;
			public Vector3 position;
			public Quaternion rotation;
		}

		[System.Serializable]
		public class EventLayout {
			public int eventId;
			public Vector3 position;
			public Quaternion rotation;
			public HitAreaShape shape;
			public Vector3 extent;
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

		public CharacterLayout AddCharacter(int layout) {
			CharacterLayout character = new CharacterLayout {
				character = 0,
				position = Vector3.zero,
				rotation = Quaternion.identity,
			};
			_layoutProfiles[layout].characters.Add(character);
			Save(true);
			return character;
		}

		public void RemoveCharacter(int layout, int index) {
			_layoutProfiles[layout].characters.RemoveAt(index);
		}

		public void UpdateEvent(
			int layout,
			int index,
			EventLayout evt
		) {
			_layoutProfiles[layout].events[index] = evt;
			Save(true);
		}

		public void UpdateEventPosition(
			int layout,
			int index, 
			Vector3 position
		) {
			_layoutProfiles[layout].events[index].position = position;
			Save(true);
		}

		public void UpdateEventRotation(
			int layout,
			int index, 
			Quaternion rotation
		) {
			_layoutProfiles[layout].events[index].rotation = rotation;
			Save(true);
		}

		public EventLayout AddEvent(int layout) {
			EventLayout eventLayout = new EventLayout {
				eventId = 0,
				position = Vector3.zero,
				rotation = Quaternion.identity,
				shape = HitAreaShape.Sphere,
				extent = new Vector3(1f,1f,1f),
			};
			_layoutProfiles[layout].events.Add(eventLayout);
			Save(true);
			return eventLayout;
		}

		public void RemoveEvent(int layout, int index) {
			_layoutProfiles[layout].events.RemoveAt(index);
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