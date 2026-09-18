using UnityEngine;

namespace hexegeer.internallib {
	public sealed class SystemSoundTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "system_sound_table";

		[System.Serializable]
		public struct SoundInfo {
			public int id;
			public string address;
		}

		[SerializeField]
		private SoundInfo[] _rows;
		public SoundInfo[] Rows => _rows;
	}
}