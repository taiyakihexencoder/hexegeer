using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace hexegeer.editor {
	public sealed class EventPointScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			errorMessages = new List<string>();
			
			EventPointSettings settings = EventPointSettings.instance;

			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			List<string> names = new List<string>();
			foreach(EventPointSettings.EventInfo info in settings.Rows) {
				if (string.IsNullOrEmpty(info.name)) {
					errorMessages.Add($"Empty name: ID={info.eventId}");
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
			EventPointSettings settings = EventPointSettings.instance;

			using (Namespace("hexegeer")) {
				using (Struct("EventId", isPartial: true)) {
					foreach(EventPointSettings.EventInfo row in settings.Rows) {
						AppendLine($"/// <summary>");
						string[] split = row.description.Split('\n');
						foreach (string line in split) {
							AppendLine($"/// {line}");
						}
						AppendLine($"/// </summary>");
						AppendLine($"public static EventId {row.name} = new EventId({row.eventId}, \"{row.name}\");");
					}
				}
			}
		}
	}
}