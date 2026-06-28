using System.Diagnostics;

namespace hexegeer.internallib {
	public static class D {
		[Conditional("UNITY_EDITOR")]
		public static void Log(object obj) {
#if UNITY_EDITOR
			UnityEngine.Debug.Log(obj);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void LogW(object obj) {
#if UNITY_EDITOR
			UnityEngine.Debug.LogWarning(obj);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void LogE(object obj) {
#if UNITY_EDITOR
			UnityEngine.Debug.LogError(obj);
#endif
		}

	}
}