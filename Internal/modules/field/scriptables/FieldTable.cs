using UnityEngine;

namespace hexegeer.internallib {
	public sealed class FieldTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "field_table";

		[System.Serializable]
		public class Row {
			public int id;
			public int contentKey;
			public string address;
			public string name;
			public string guid;
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 boundsMin;
			public Vector3 boundsMax;
		}

		[SerializeField]
		private Row[] _rows;
		public Row[] Rows => _rows;
	}
}