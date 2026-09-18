using UnityEngine;

namespace hexegeer.internallib {
	public sealed class EnvironmentSoundTable : ScriptableObject {
		public const string RESOURCE_ADDRESS = "environment_sound_table";

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