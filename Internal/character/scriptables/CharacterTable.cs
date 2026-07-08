using UnityEngine;

namespace hexegeer.internallib {
	public sealed class CharacterTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "character_table";

		[System.Serializable]
		public class Character {
			public int id;
			public string name;
			public int collider;
			public int belongsTo;
			public int collidesWith;
			public bool hasObservationPoint;
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
			public int[] characterIndices;
		}

		[SerializeField]
		private int _physicsObjectLayer;
		public int PhysicsObjectLayer => _physicsObjectLayer;

		[SerializeField]
		private int _physicsObjectCollides;
		public int PhysicsObjectCollides => _physicsObjectCollides;

		[SerializeField]
		private Character[] _characters;
		public Character[] Characters => _characters;

		[SerializeField]
		private CharacterCollider[] _colliders;
		public CharacterCollider[] Colliders => _colliders;

		[SerializeField]
		private KeyTable[] _keyTables;
		public KeyTable[] KeyTables => _keyTables;

		public CharacterCollider GetCollider(int id) {
			foreach(CharacterCollider collider in _colliders) {
				if (collider.id == id) {
					return collider;
				}
			}
			return null;
		}
	}
}