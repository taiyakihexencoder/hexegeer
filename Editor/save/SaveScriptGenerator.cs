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
			AppendLine($"using System.Collections.Generic;");
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

					using (Function("partial void SetDefault()")) {
						foreach(SaveSettings.SaveParameter parameter in settings.User.parameters) {
							AppendLine($"{parameter.name} = {ValueUser(parameter.type, parameter.defaultValue)};");
						}
					}
				}

				using (Class("UserSaveAccessor : IUserSaveAccessor")) {
					AppendLine($"PersistentData.ISerializer<UserSaveParameter> IUserSaveAccessor.serializer => new Serializer();");
					AppendLine($"PersistentData.IDeserializer<UserSaveParameter> IUserSaveAccessor.deserializer => new Deserializer();");

					AppendLine();

					using (Class("Serializer : PersistentData.ISerializer<UserSaveParameter>", isSealed: true, visibility: "private")) {
						using (Function("byte[] PersistentData.ISerializer<UserSaveParameter>.Serialize(in UserSaveParameter data)")) {
							AppendLine($"int length = 3;");
							AppendLine($"Dictionary<string, byte[]> strTable = new Dictionary<string, byte[]>();");

							List<SaveSettings.SaveParameter> parameters = settings.User.parameters;
							parameters.Sort(SortByVersion);

							foreach(SaveSettings.SaveParameter parameter in parameters) {
								if (parameter.type == SaveSettings.SaveParameterType.String) {
									AppendLine($"strTable.Add(\"{parameter.name}\", System.Text.Encoding.UTF8.GetBytes(data.{parameter.name}.ToString()));");
									AppendLine($"length += strTable[\"{parameter.name}\"].Length+1; // {parameter.name}");
								} else {
									AppendLine($"length += {TypeSize(parameter.type)}; // {parameter.name}");
								}
							}
							AppendLine($"byte[] raw = new byte[length];");
							AppendLine($"string[] currentVersion = UnityEngine.Application.version.Split('.');");
							AppendLine($"raw[0] = byte.Parse(currentVersion[0]);");
							AppendLine($"raw[1] = byte.Parse(currentVersion[1]);");
							AppendLine($"raw[2] = byte.Parse(currentVersion[2]);");
							AppendLine($"int offset = 3;");
							AppendLine();
							foreach(SaveSettings.SaveParameter parameter in parameters) {
								AppendLine($"// {parameter.name} ({TypeNameUser(parameter.type)})");
								switch(parameter.type) {
									case SaveSettings.SaveParameterType.Int: {
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}), 0, raw, offset, {sizeof(int)});");
										AppendLine($"offset += {sizeof(int)};");
										break;
									}
									case SaveSettings.SaveParameterType.Long: {
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}), 0, raw, offset, {sizeof(long)});");
										AppendLine($"offset += {sizeof(long)};");
										break;
									}
									case SaveSettings.SaveParameterType.Boolean: {
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}), 0, raw, offset, {sizeof(bool)});");
										AppendLine($"offset += {sizeof(bool)};");
										break;
									}
									case SaveSettings.SaveParameterType.String: {
										AppendLine($"raw[offset] = (byte)strTable[\"{parameter.name}\"].Length;");
										AppendLine($"System.Array.Copy(strTable[\"{parameter.name}\"], 0, raw, offset+1, strTable[\"{parameter.name}\"].Length);");
										AppendLine($"offset += strTable[\"{parameter.name}\"].Length + 1;");
										break;
									}
									case SaveSettings.SaveParameterType.Float: {
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										break;
									}
									case SaveSettings.SaveParameterType.Vector2: {
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.x), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.y), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										break;
									}
									case SaveSettings.SaveParameterType.Vector3: {
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.x), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.y), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.z), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										break;
									}
									case SaveSettings.SaveParameterType.Color: {
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.x), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.y), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.z), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										AppendLine($"System.Array.Copy(System.BitConverter.GetBytes(data.{parameter.name}.w), 0, raw, offset, {sizeof(float)});");
										AppendLine($"offset += {sizeof(float)};");
										break;
									}
								}
								AppendLine();
							}
							AppendLine($"return raw;");
						}
					}

					using (Class("Deserializer : PersistentData.IDeserializer<UserSaveParameter>", isSealed: true, visibility: "private")) {
						using (Function("UserSaveParameter PersistentData.IDeserializer<UserSaveParameter>.Deserialize(in byte[] raw)")) {
							AppendLine($"Version version = new Version {{");
							using (Indent) {
								AppendLine($"major = raw[0],");
								AppendLine($"minor = raw[1],");
								AppendLine($"patch = raw[2],");
							}
							AppendLine($"}};");

							AppendLine("UserSaveParameter data = UserSaveParameter.defaultValue;");
							AppendLine($"int offset = 3;");
							List<SaveSettings.SaveParameter> parameters = settings.User.parameters;
							parameters.Sort(SortByVersion);


							int currentMajor = 0;
							int currentMinor = 0;
							int currentPatch = 0;
							string indent = "";
							foreach(SaveSettings.SaveParameter parameter in parameters) {
								if (parameter.version.major > currentMajor || parameter.version.minor > currentMinor || parameter.version.patch > currentPatch) {
									if (currentMajor > 0 || currentMinor > 0) {
										AppendLine($"}}");
									} else {
										indent = "\t";
									}

									currentMajor = parameter.version.major;
									currentMinor = parameter.version.minor;
									currentPatch = parameter.version.patch;

									AppendLine($"if (version.major > {currentMajor} || (version.major == {currentMajor} && version.minor > {currentMinor}) || (version.major == {currentMajor} && version.minor == {currentMinor} && version.patch >= {currentPatch})) {{");
								}
								switch(parameter.type) {
									case SaveSettings.SaveParameterType.Int: {
										AppendLine($"{indent}data.{parameter.name} = System.BitConverter.ToInt32(raw, offset);");
										AppendLine($"{indent}offset += {sizeof(int)};");
										break;
									}
									case SaveSettings.SaveParameterType.Long: {
										AppendLine($"{indent}data.{parameter.name} = System.BitConverter.ToInt64(raw, offset);");
										AppendLine($"{indent}offset += {sizeof(long)};");
										break;
									}
									case SaveSettings.SaveParameterType.Boolean: {
										AppendLine($"{indent}data.{parameter.name} = System.BitConverter.ToBoolean(raw, offset);");
										AppendLine($"{indent}offset += {sizeof(bool)};");
										break;
									}
									case SaveSettings.SaveParameterType.String: {
										AppendLine($"{indent}data.{parameter.name} = new FixedString64Bytes(System.Text.Encoding.UTF8.GetString(raw, offset+1, raw[offset]));");
										AppendLine($"{indent}offset += 1 + raw[offset];");
										break;
									}
									case SaveSettings.SaveParameterType.Float: {
										AppendLine($"{indent}data.{parameter.name} = System.BitConverter.ToSingle(raw, offset);");
										AppendLine($"{indent}offset += {sizeof(float)};");
										break;
									}
									case SaveSettings.SaveParameterType.Vector2: {
										AppendLine($"{indent}data.{parameter.name} = new float2(System.BitConverter.ToSingle(raw, offset), System.BitConverter.ToSingle(raw, offset + {sizeof(float)}));");
										AppendLine($"{indent}offset += {sizeof(float)*2};");
										break;
									}
									case SaveSettings.SaveParameterType.Vector3: {
										AppendLine($"{indent}data.{parameter.name} = new float3(System.BitConverter.ToSingle(raw, offset), System.BitConverter.ToSingle(raw, offset + {sizeof(float)}), System.BitConverter.ToSingle(raw, offset + {sizeof(float)*2}));");
										AppendLine($"{indent}offset += {sizeof(float)*3};");
										break;
									}
									case SaveSettings.SaveParameterType.Color: {
										AppendLine($"{indent}data.{parameter.name} = new float4(System.BitConverter.ToSingle(raw, offset), System.BitConverter.ToSingle(raw, offset + {sizeof(float)}), System.BitConverter.ToSingle(raw, offset + {sizeof(float)*2}), System.BitConverter.ToSingle(raw, offset + {sizeof(float)*3}));");
										AppendLine($"{indent}offset += {sizeof(float)*4};");
										break;
									}
								}
								AppendLine();
							}
							if (currentMajor > 0 || currentMinor > 0) {
								AppendLine($"}}");
							}
							AppendLine($"return data;");
						}
					}
				}
			}
		}

		private int SortByVersion(SaveSettings.SaveParameter a, SaveSettings.SaveParameter b) {
			int major = a.version.major.CompareTo(b.version.major);
			if (major != 0) return major;
			int minor = a.version.minor.CompareTo(b.version.minor);
			if (minor != 0) return minor;
			return a.version.patch.CompareTo(b.version.patch);
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
				case SaveSettings.SaveParameterType.Float: { return $"{value}"; }
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
				case SaveSettings.SaveParameterType.Float: { return $"{value}"; }
				case SaveSettings.SaveParameterType.Vector2: { return $"new float2({value})";}
				case SaveSettings.SaveParameterType.Vector3: { return $"new float3({value})";}
				case SaveSettings.SaveParameterType.Color: { return $"new float4({value})"; }
				default: return "default";
			}
		}

		private int TypeSize(SaveSettings.SaveParameterType type) {
			switch(type) {
				case SaveSettings.SaveParameterType.Int: { return sizeof(int); }
				case SaveSettings.SaveParameterType.Long: { return sizeof(long); }
				case SaveSettings.SaveParameterType.Boolean: { return sizeof(bool); }
				case SaveSettings.SaveParameterType.String: { return 0; }
				case SaveSettings.SaveParameterType.Float: { return sizeof(float); }
				case SaveSettings.SaveParameterType.Vector2: { return sizeof(float) * 2; }
				case SaveSettings.SaveParameterType.Vector3: { return sizeof(float) * 3; }
				case SaveSettings.SaveParameterType.Color: { return sizeof(float) * 4;}
				default: return 0;
			}
		}
	}
}