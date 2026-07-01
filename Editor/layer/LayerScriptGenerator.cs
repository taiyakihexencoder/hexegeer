using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace hexegeer.editor {
	public sealed class LayerScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			errorMessages = new List<string>();
			List<string> defaultNames = new List<string>(System.Enum.GetNames(typeof(DefaultLayer)));
			List<string> names = new List<string>(defaultNames);

			LayerSettings settings = LayerSettings.instance;

			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			for(int i = 0; i < LayerSettings.LAYER_COUNT; ++i) {
				string name = settings.LayerName(i);
				if (defaultNames.Contains(name)) {
					continue;
				}

				if (string.IsNullOrEmpty(name)) {
					continue;
				}

				if (!regex.IsMatch(name)) {
					errorMessages.Add($"Invalid name: {name}.");
				}

				if (names.Contains(name)) {
					errorMessages.Add($"Duplicated name: {name}.");
				}

				names.Add(name);
			}

			return errorMessages.Count == 0;
		}

		protected override void WriteScript() {
			LayerSettings settings = LayerSettings.instance;
			string[] layerNames = settings.LayerNames;

			List<string> defaultNames = new List<string>(System.Enum.GetNames(typeof(DefaultLayer)));

			using(Namespace("hexegeer")) {

				// レイヤー名
				using(Class("Layer", true, true)) {
					for (int i = 0; i < LayerSettings.LAYER_COUNT; ++i) {
						string name = layerNames[i];
						if (string.IsNullOrEmpty(name) || defaultNames.Contains(name)){
							continue;
						}
						int index = settings.LayerIndex(i);
						AppendLine($"public const uint {name} = {1 << index}u;");
					}
				}

				AppendLine();

				// 衝突判定
				using (Class("LayerCollide", true, true)) {
					for (int i = 0; i < LayerSettings.LAYER_COUNT; ++i) {
						string name = layerNames[i];
						
						if (string.IsNullOrEmpty(name) || defaultNames.Contains(name)){
							continue;
						}

						int index = settings.LayerIndex(i);
						List<string> names = new List<string>();
						for (int j = 0; j < LayerSettings.LAYER_COUNT; ++j) {
							int toIndex = settings.LayerIndex(j);
							if (!settings.Table(index, toIndex)) {
								continue;
							}

							string toName = layerNames[j];
							if (string.IsNullOrEmpty(toName)) {
								continue;
							}

							names.Add(toName);
						}

						if (names.Count > 0) {
							AppendLine($"public const uint {name} = Layer.{string.Join(" | Layer.", names)};");
						} else {
							AppendLine($"public const uint {name} = 0u;");
						}
					}
				}
			}
		}
	}
}