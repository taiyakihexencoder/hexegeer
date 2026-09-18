using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace hexegeer.editor {
	internal class SystemSoundScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			errorMessages = new List<string>();

			SoundSettings settings = SoundSettings.instance;
			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			List<string> names = new List<string>();
			foreach(SoundSettings.SoundInfo info in settings.SystemSoundList) {
				if (string.IsNullOrEmpty(info.name)) {
					errorMessages.Add($"Empty name: ID={info.id}");
				} else if (!regex.IsMatch(info.name)) {
					errorMessages.Add($"Invalid name: {info.name}");
				} else if (names.Contains(info.name)) {
					errorMessages.Add($"Duplicated name: {info.name}");
				} else {
					names.Add(info.name);
				}
			}

			return errorMessages.Count == 0;
		}

		protected override void WriteScript() {
			SoundSettings settings = SoundSettings.instance;

			using (Namespace("hexegeer")) {
				using (Struct("SoundId", isPartial: true)) {
					foreach(SoundSettings.SoundInfo info in settings.SystemSoundList) {
						AppendLine($"public static SoundId {info.name} = new SoundId({info.id}, \"{info.name}\");");
					}
				}
			}

		}
	}
}