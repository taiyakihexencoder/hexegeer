using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace hexegeer.editor {
	public sealed class CharacterScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> messages) {
			messages = new List<string>();
			CharacterSettings settings = CharacterSettings.instance;

			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			List<string> names = new List<string>();
			foreach(CharacterSettings.CharacterData character in settings.Characters) {
				if (string.IsNullOrEmpty(character.name)) {
					messages.Add($"Empty name: ID={character.id}");
				} else if (!regex.IsMatch(character.name)) {
					messages.Add($"Invalid name: {character.name}");
				} else if (names.Contains(character.name)) {
					messages.Add($"Duplicated name: {character.name}");
				} else {
					names.Add(character.name);
				}
			}
			return messages.Count == 0;
		}

		protected override void WriteScript() {
			CharacterSettings settings = CharacterSettings.instance;

			using (Namespace("hexegeer")) {
				using (Struct("CharacterId", isPartial: true, isStatic: false)) {
					foreach(CharacterSettings.CharacterData character in settings.Characters) {
						AppendLine($"public static readonly CharacterId {character.name} = new CharacterId({character.id}, \"{character.name}\");");
					}
				}
			}
		}
	}
}