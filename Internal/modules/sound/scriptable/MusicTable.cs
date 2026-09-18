using UnityEngine;

namespace hexegeer.internallib {
	public sealed class MusicTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "music_table";

		[System.Serializable]
		public struct MusicInfo {
			public int id;
			public string address;
		}

		[SerializeField]
		private MusicInfo[] _rows;
		public MusicInfo[] Rows => _rows;
	}
}