namespace hexegeer {
	using internallib;
	public static class PermanentData {
		public static void Set(string key, bool value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, bool[] value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, int value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, int[] value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, long value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, long[] value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, float value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, float[] value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set(string key, string value) { PermanentDataAdapter.Prefs.Set(key, value); }
		public static void Set<T>(string key, T value) { PermanentDataAdapter.Prefs.Set(key, value); }

		public static bool TryGet(string key, out bool value) { return PermanentDataAdapter.Prefs.TryGetFlag(key, out value); }
		public static bool TryGet(string key, out int value) { return PermanentDataAdapter.Prefs.TryGetInt(key, out value); }
		public static bool TryGet(string key, out long value) { return PermanentDataAdapter.Prefs.TryGetLong(key, out value); }
		public static bool TryGet(string key, out float value) { return PermanentDataAdapter.Prefs.TryGetFloat(key, out value); }
		public static bool TryGet(string key, out string value) { return PermanentDataAdapter.Prefs.TryGetString(key, out value); }
		public static bool TryGet<T>(string key, out T value) { return PermanentDataAdapter.Prefs.TryGet<T>(key, out value); }

		public static bool GetFlag(string key, bool defaultValue = false) { return PermanentDataAdapter.Prefs.GetFlag(key, defaultValue); }
		public static bool[] GetFlagArray(string key, bool defaultValue = false) { return PermanentDataAdapter.Prefs.GetFlagArray(key, defaultValue); }
		public static int GetInt(string key, int defaultValue = 0) { return PermanentDataAdapter.Prefs.GetInt(key, defaultValue); }
		public static int[] GetIntArray(string key, int defaultValue = 0) { return PermanentDataAdapter.Prefs.GetIntArray(key, defaultValue); }
		public static long GetLong(string key, long defaultValue = 0L) { return PermanentDataAdapter.Prefs.GetLong(key, defaultValue); }
		public static long[] GetLongArray(string key, long defaultValue = 0L) { return PermanentDataAdapter.Prefs.GetLongArray(key, defaultValue); }
		public static float GetFloat(string key, float defaultValue = 0f) { return PermanentDataAdapter.Prefs.GetFloat(key, defaultValue); }
		public static float[] GetFloatArray(string key, float defaultValue = 0f) { return PermanentDataAdapter.Prefs.GetFloatArray(key, defaultValue); }
		public static string GetString(string key, string defaultValue = null) { return PermanentDataAdapter.Prefs.GetString(key, defaultValue); }
		public static T Get<T>(string key, T defaultValue = default) { return PermanentDataAdapter.Prefs.Get<T>(key, defaultValue); }

		public static void Save() { PermanentDataAdapter.Prefs.Save(); }

		public static void DeleteKey(string key) { PermanentDataAdapter.Prefs.DeleteKey(key); }

		public static void DeleteAll() { PermanentDataAdapter.Prefs.DeleteAll(); }

	}
}