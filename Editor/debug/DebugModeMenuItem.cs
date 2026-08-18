using UnityEditor;

namespace hexegeer.editor {
	public sealed class DebugModeMenuItem {
		private const string PATH = "Hexegeer/Debug Mode";

		private DebugModeMenuItem(){ }

		[MenuItem(PATH, priority = 100000)]
		private static void DebugMenuFlag() {
			bool current = DebugSettings.instance.DebugMode;
			Menu.SetChecked(PATH, !current);
			DebugSettings.instance.SetDebugMode(!current);
		}
	}
}