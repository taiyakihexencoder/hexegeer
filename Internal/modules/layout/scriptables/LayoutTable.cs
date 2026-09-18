using System.Collections.Generic;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class LayoutTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "layout_table";

		[System.Serializable]
		public class Profile {
			[SerializeField]
			private int _contentKey;
			public int ContentKey => _contentKey;

			[SerializeField]
			private List<int> _characterIds;
			public List<int> CharacterIds => _characterIds;

			[SerializeField]
			private List<CharacterLayout> _characters;
			public List<CharacterLayout> Characters => _characters;

			[SerializeField]
			private List<EventLayout> _events;
			public List<EventLayout> Events => _events;
		}

		[System.Serializable]
		public class CharacterLayout {
			[SerializeField]
			private int _id;
			public int Id => _id;

			[SerializeField]
			private Vector3 _position;
			public Vector3 Position => _position;

			[SerializeField]
			private Quaternion _rotation;
			public Quaternion Rotation => _rotation;
		}

		[System.Serializable]
		public class EventLayout {
			[SerializeField]
			private int _eventId;
			public int EventId => _eventId;

			[SerializeField]
			private Vector3 _position;
			public Vector3 Position => _position;

			[SerializeField]
			private Quaternion _rotation;
			public Quaternion Rotation => _rotation;

			[SerializeField]
			private HitAreaShape _shape;
			public HitAreaShape Shape => _shape;

			[SerializeField]
			private Vector3 _extent;
			public Vector3 Extent => _extent;
		}

		[SerializeField]
		private List<Profile> _layoutProfiles;
		public List<Profile> LayoutProfiles => _layoutProfiles;
	}
}
