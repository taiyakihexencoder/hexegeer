using System.Collections.Generic;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class DamageObjectTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "damage_object_table";
		
		[System.Serializable]
		public class DamageObject {
			public int id;
			public string name;
			public int collider;
			public int belongsTo;
			public int collidesWith;
		}

		[System.Serializable]
		public class DamageObjectCollider {
			public int id;
			public string name;
			public HitAreaShape shape;
			public Vector3 extent;
		}

		[System.Serializable]
		public class KeyTable {
			public int key;
			public List<int> indices;
		}

		[SerializeField]
		private List<DamageObject> _damageObjects;
		public List<DamageObject> DamageObjects => _damageObjects;

		[SerializeField]
		private List<DamageObjectCollider> _colliders;
		public List<DamageObjectCollider> Colliders => _colliders;


		[SerializeField]
		private KeyTable[] _contentKeyTable;
		public KeyTable[] ContentKeyTable => _contentKeyTable;

		public DamageObjectCollider GetCollider(int id) {
			return _colliders.Find(_ => _.id == id);
		}
	}
}
