using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace hexegeer.editor {
	internal class MusicScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			errorMessages = new List<string>();

			MusicSettings settings = MusicSettings.instance;
			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			List<string> names = new List<string>();
			foreach(MusicSettings.MusicInfo info in settings.MusicList) {
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
			MusicSettings settings = MusicSettings.instance;

			using (Namespace("hexegeer")) {
				using (Struct("MusicId", isPartial: true)) {
					foreach(MusicSettings.MusicInfo info in settings.MusicList) {
						if (!string.IsNullOrEmpty(info.description)) {
							AppendLine($"/// <summary>");
							string[] lines = info.description.Split("\n");
							foreach(string line in lines) {
								AppendLine($"/// {line}");
							}
							AppendLine($"/// </summary>");
						}
						AppendLine($"public static MusicId {info.name} = new MusicId({info.id}, \"{info.name}\");");
					}
				}
			}

		}
	}
}