using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

namespace hexegeer.editor {
	public sealed class LocalizeListGenerator : SourceCodeGenerator {
		protected override void WriteScript() {
			using (Namespace("hexegeer")) {
				using (Struct("LocalizeKey", isPartial: true)) {
					foreach(string guid in AssetDatabase.FindAssets($"t:{typeof(StringTableCollection).Name}")) {
						string path = AssetDatabase.GUIDToAssetPath(guid);
						StringTableCollection collection = AssetDatabase.LoadAssetAtPath<StringTableCollection>(path);
						SharedTableData sharedData = collection.SharedData;
						if (sharedData == null) {
							continue;
						}

						string tableName = collection.name;

						AppendLine($"// {tableName}");
						foreach(SharedTableData.SharedTableEntry entry in sharedData.Entries) {
							string key = entry.Key;
							long id = entry.Id;
							AppendLine($"public static LocalizeKey {key} = new LocalizeKey({id}, \"{key}\");");
						}
						AppendLine();
					}
				}
			}
		}
	}
}