using System.Collections.Generic;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class DamageObjectTable : ScriptableObject {
		[System.Serializable]
		public class DamageObject {
			public int id;
			public string name;
			public int collider;
		}

		[System.Serializable]
		public class DamageObjectCollider {
			public int id;
			public string name;
			public Shape shape;
			public Vector3 extent;
			public int belongsTo;
			public int collidesWith;
		}

		public enum Shape {
			Sphere,
			Box,
			Cylinder,
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
