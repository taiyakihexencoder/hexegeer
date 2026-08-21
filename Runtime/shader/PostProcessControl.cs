using UnityEngine;
using UnityEngine.Rendering;

namespace hexegeer {
	public sealed class PostProcessControl : MonoBehaviour {
		private static PostProcessControl _instance;
		private VolumeProfile _profile;

		private void Awake() {
			_instance = this;
			_profile = GetComponent<VolumeProfile>();
		}

		public static bool TryGetProfile<T>(out T volume) where T: VolumeComponent {
			return _instance._profile.TryGet(out volume);
		}
	}
}