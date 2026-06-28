using UnityEngine;

namespace hexegeer.internallib {
	public static class AppUtil {
		public delegate void QuittingHandler();

		public static QuittingHandler quitting = null;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Init() {
			Application.quitting += Quitting;
		}

		private static void Quitting() {
			if (quitting != null) {
				quitting.Invoke();
				quitting = null;
			}
		}

		public static void Quit() {
#if UNITY_EDITOR
			Quitting();
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}