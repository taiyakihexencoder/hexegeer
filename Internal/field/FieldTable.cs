using UnityEngine;

namespace hexegeer.internallib {
	public sealed class FieldTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "field_table";

		[System.Serializable]
		internal class Row {
			public int id;
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
		internal Row[] Rows => _rows;
	}
}