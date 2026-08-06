using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class SaveScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			errorMessages = new List<string>();

			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			SaveSettings settings = SaveSettings.instance;

			List<string> names = new List<string>();
			foreach(SaveSettings.SaveParameter parameter in settings.Global.parameters) {
				if (string.IsNullOrEmpty(parameter.name)) {
					errorMessages.Add($"Global - Empty name: {parameter.type}");
				} else if (!regex.IsMatch(parameter.name)) {
					errorMessages.Add($"Global - Invalid name: {parameter.name}");
				} else if (names.Contains(parameter.name)) {
					errorMessages.Add($"Global - Duplicated name: {parameter.name}");
				} else {
					names.Add(parameter.name);
				}
			}

			names.Clear();
			foreach(SaveSettings.SaveParameter parameter in settings.User.parameters) {
				if (string.IsNullOrEmpty(parameter.name)) {
					errorMessages.Add($"User - Empty name: {parameter.type}");
				} else if (!regex.IsMatch(parameter.name)) {
					errorMessages.Add($"User - Invalid name: {parameter.name}");
				} else if (names.Contains(parameter.name)) {
					errorMessages.Add($"User - Duplicated name: {parameter.name}");
				} else {
					names.Add(parameter.name);
				}
			}

			return errorMessages.Count == 0;
		}

		protected override void WriteScript() {
			SaveSettings settings = SaveSettings.instance;
			AppendLine($"using hexegeer.internallib;");
			AppendLine($"using Unity.Collections;");
			AppendLine($"using Unity.Mathematics;");
			AppendLine();
			using (Namespace("hexegeer")) {
				using (Class("GlobalSaveAccessor", isPartial: true, isStatic: true)) {
					foreach(SaveSettings.SaveParameter parameter in settings.Global.parameters) {
						AppendLine($"public static {TypeNameGlobal(parameter.type)} {parameter.name} {{");
						using (Indent) {
							AppendLine($"get => SaveInternal.Global.Get{parameter.type}(\"pref_{parameter.name}\", {ValueGlobal(parameter.type, parameter.defaultValue)});");
							AppendLine($"set => SaveInternal.Global.Set(\"pref_{parameter.name}\", value);");
						}
						AppendLine($"}}");
						AppendLine();
					}
				}

				AppendLine();

				using (Struct("UserSaveParameter", isPartial: true, isStatic: false, isReadonly: false)) {
					foreach(SaveSettings.SaveParameter parameter in settings.User.parameters) {
						AppendLine($"public {TypeNameUser(parameter.type)} {parameter.name};");
					}

					AppendLine();

					using (Function("public static UserSaveParameter Default()")) {
						AppendLine($"return new UserSaveParameter {{");
						using (Indent) {
							foreach(SaveSettings.SaveParameter parameter in settings.User.parameters) {
								AppendLine($"{parameter.name} = {ValueUser(parameter.type, parameter.defaultValue)},");
							}
						}
						AppendLine($"}};");
					}
				}
			}
		}

		private string TypeNameGlobal(SaveSettings.SaveParameterType type) {
			switch(type) {
				case SaveSettings.SaveParameterType.Int: { return "int"; }
				case SaveSettings.SaveParameterType.Long: { return "long"; }
				case SaveSettings.SaveParameterType.Boolean: { return "bool"; }
				case SaveSettings.SaveParameterType.String: { return "string"; }
				case SaveSettings.SaveParameterType.Float: { return "float"; }
				case SaveSettings.SaveParameterType.Vector2: { return "Vector2";}
				case SaveSettings.SaveParameterType.Vector3: { return "Vector3";}
				case SaveSettings.SaveParameterType.Color: { return "Color"; }
				default: return "";
			}
		}

		private string ValueGlobal(SaveSettings.SaveParameterType type, string value) {
			switch(type) {
				case SaveSettings.SaveParameterType.Int: { return value; }
				case SaveSettings.SaveParameterType.Long: { return $"{value}L"; }
				case SaveSettings.SaveParameterType.Boolean: { return value == "true" ? "true" : "false"; }
				case SaveSettings.SaveParameterType.String: { return $"\"{value}\""; }
				case SaveSettings.SaveParameterType.Float: { return $"{value}f"; }
				case SaveSettings.SaveParameterType.Vector2: { return $"new Vector2({value})";}
				case SaveSettings.SaveParameterType.Vector3: { return $"new Vector3({value})";}
				case SaveSettings.SaveParameterType.Color: { return $"new Color({value})"; }
				default: return "default";
			}
		}

		private string TypeNameUser(SaveSettings.SaveParameterType type) {
			switch(type) {
				case SaveSettings.SaveParameterType.Int: { return "int"; }
				case SaveSettings.SaveParameterType.Long: { return "long"; }
				case SaveSettings.SaveParameterType.Boolean: { return "bool"; }
				case SaveSettings.SaveParameterType.String: { return "FixedString64Bytes"; }
				case SaveSettings.SaveParameterType.Float: { return "float"; }
				case SaveSettings.SaveParameterType.Vector2: { return "float2";}
				case SaveSettings.SaveParameterType.Vector3: { return "float3";}
				case SaveSettings.SaveParameterType.Color: { return "float4"; }
				default: return "";
			}
		}

		private string ValueUser(SaveSettings.SaveParameterType type, string value) {
			switch(type) {
				case SaveSettings.SaveParameterType.Int: { return value; }
				case SaveSettings.SaveParameterType.Long: { return $"{value}L"; }
				case SaveSettings.SaveParameterType.Boolean: { return value == "true" ? "true" : "false"; }
				case SaveSettings.SaveParameterType.String: { return $"new FixedString64Bytes(\"{value}\")"; }
				case SaveSettings.SaveParameterType.Float: { return $"{value}f"; }
				case SaveSettings.SaveParameterType.Vector2: { return $"new float2({value})";}
				case SaveSettings.SaveParameterType.Vector3: { return $"new float3({value})";}
				case SaveSettings.SaveParameterType.Color: { return $"new float4({value})"; }
				default: return "default";
			}
		}

	}
}