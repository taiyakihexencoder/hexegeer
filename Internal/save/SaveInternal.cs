using UnityEngine;

namespace hexegeer.internallib {
	public static class SaveInternal {
		public static class Global {
			public static void Set(string key, int value) { PlayerPrefs.SetInt(key, value); }
			public static void Set(string key, long value) {
				PlayerPrefs.SetInt($"{key}.u", (int)(value >> 32));
				PlayerPrefs.SetInt($"{key}.d", (int)(value & 0xFFFFFFFFL));
			}
			public static void Set(string key, bool value) { PlayerPrefs.SetInt(key, value ? 1 : 0); }
			public static void Set(string key, float value) { PlayerPrefs.SetFloat(key, value); }
			public static void Set(string key, string value) { PlayerPrefs.SetString(key, value); }
			public static void Set(string key, Vector2 value) {
				PlayerPrefs.SetFloat($"{key}.x", value.x);
				PlayerPrefs.SetFloat($"{key}.y", value.y);
			}

			public static void Set(string key, Vector3 value) {
				PlayerPrefs.SetFloat($"{key}.x", value.x);
				PlayerPrefs.SetFloat($"{key}.y", value.y);
				PlayerPrefs.SetFloat($"{key}.z", value.z);
			}

			public static void Set(string key, Color value) {
				PlayerPrefs.SetFloat($"{key}.r", value.r);
				PlayerPrefs.SetFloat($"{key}.g", value.g);
				PlayerPrefs.SetFloat($"{key}.b", value.b);
				PlayerPrefs.SetFloat($"{key}.a", value.a);
			}

			public static int GetInt(string key, int defaultValue) { return PlayerPrefs.GetInt(key, defaultValue); }
			public static long GetLong(string key, long defaultValue) {
				long u = PlayerPrefs.GetInt($"{key}.u", (int)(defaultValue >> 32));
				long d = PlayerPrefs.GetInt($"{key}.d", (int)(defaultValue & 0xFFFFFFFFL));
				return (u << 32) & d;
			}
			public static bool GetBoolean(string key, bool defaultValue) { return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1; }
			public static float GetFloat(string key, float defaultValue) { return PlayerPrefs.GetFloat(key, defaultValue); }
			public static string GetString(string key, string defaultValue) { return PlayerPrefs.GetString(key, defaultValue); }
			public static Vector2 GetVector2(string key, Vector2 defaultValue) { 
				return new Vector2(
					PlayerPrefs.GetFloat($"{key}.x", defaultValue.x),
					PlayerPrefs.GetFloat($"{key}.y", defaultValue.y)
				); 
			}
			public static Vector3 GetVector3(string key, Vector3 defaultValue) { 
				return new Vector3(
					PlayerPrefs.GetFloat($"{key}.x", defaultValue.x),
					PlayerPrefs.GetFloat($"{key}.y", defaultValue.y),
					PlayerPrefs.GetFloat($"{key}.z", defaultValue.z)
				); 
			}
			public static Color GetColor(string key, Color defaultValue) { 
				return new Color(
					PlayerPrefs.GetFloat($"{key}.r", defaultValue.r),
					PlayerPrefs.GetFloat($"{key}.g", defaultValue.g),
					PlayerPrefs.GetFloat($"{key}.b", defaultValue.b),
					PlayerPrefs.GetFloat($"{key}.a", defaultValue.a)
				); 
			}
		}
	}
}