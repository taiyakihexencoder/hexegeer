using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Debug/DebugSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class DebugSettings : ScriptableSingleton<DebugSettings> {
		[SerializeField]
		private bool _debugMode = false;
		public bool DebugMode => _debugMode;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void StartDebugMode() {
			if (instance.DebugMode) {
				internallib.DebugMode.GenerateInstance();
			}
		}

		public void SetDebugMode(bool value) {
			_debugMode = value;
			Save(true);
		}
	}
}