using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace hexegeer.editor {
	public sealed class DamageObjectScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			errorMessages = new List<string>();
			DamageObjectSettings settings = DamageObjectSettings.instance;

			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			List<string> names = new List<string>();
			foreach (DamageObjectSettings.DamageObjectData damageObject in settings.Rows) {
				if (string.IsNullOrEmpty(damageObject.name)) {
					errorMessages.Add($"Empty name: ID={damageObject.id}");
				} else if (!regex.IsMatch(damageObject.name)) {
					errorMessages.Add($"Invalid name: {damageObject.name}");
				} else if (names.Contains(damageObject.name)) {
					errorMessages.Add($"Duplicated name: {damageObject.name}");
				} else {
					names.Add(damageObject.name);
				}
			}
			return errorMessages.Count == 0;
		}

		protected override void WriteScript() {
			DamageObjectSettings settings = DamageObjectSettings.instance;

			using (Namespace("hexegeer")) {
				using (Struct("DamageObjectId", isPartial: true, isStatic: false)) {
					foreach(DamageObjectSettings.DamageObjectData damageObject in settings.Rows) {
						AppendLine($"public static readonly DamageObjectId {damageObject.name} = new DamageObjectId({damageObject.id}, \"{damageObject.name}\");");
					}
				}
			}
		}
	}
}