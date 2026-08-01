using System.Collections.Generic;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class CharacterTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "character_table";

		[System.Serializable]
		public class Character {
			public int id;
			public string name;
			public string modelAsset;
			public int collider;
			public int belongsTo;
			public int collidesWith;
			public bool hasObservationPoint;

			public ModelProfile modelProfile;
		}

		[System.Serializable]
		public class ModelProfile {
			public string modelAsset;
			public List<string> overrideAnimations;
			public List<string> additiveAnimations;
			public List<string> baseAnimations;
		}

		[System.Serializable]
		public class CharacterCollider {
			public int id;
			public string name;
			public float radius;
			public float height;
		}

		[System.Serializable]
		public class KeyTable {
			public int key;
			public List<int> characterIndices;
		}

		[SerializeField]
		private int _physicsObjectLayer;
		public int PhysicsObjectLayer => _physicsObjectLayer;

		[SerializeField]
		private int _physicsObjectCollides;
		public int PhysicsObjectCollides => _physicsObjectCollides;

		[SerializeField]
		private List<Character> _characters;
		public List<Character> Characters => _characters;

		[SerializeField]
		private List<CharacterCollider> _colliders;
		public List<CharacterCollider> Colliders => _colliders;

		[SerializeField]
		private KeyTable[] _keyTables;
		public KeyTable[] KeyTables => _keyTables;

		public CharacterCollider GetCollider(int id) {
			return _colliders.Find(_ => _.id == id);
		}
	}
}