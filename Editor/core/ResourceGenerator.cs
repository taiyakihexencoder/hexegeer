using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace hexegeer.editor {
	internal abstract class ResourceGenerator<T> where T : ScriptableObject {
		internal virtual string AutoGeneratePath => $"com.hexengine.hexegeer{Path.DirectorySeparatorChar}Auto-Generate{Path.DirectorySeparatorChar}res{Path.DirectorySeparatorChar}";

		internal void Generate(string fileName) {
			string genPath = AutoGeneratePath + fileName;

			string[] splits = genPath.Split(new char[] { '/', '\\', ':' });
			string basePath = Application.dataPath;
			for (int i = 0; i < splits.Length -1; ++i) {
				basePath += $"{Path.DirectorySeparatorChar}{splits[i]}";
				if (!Directory.Exists(basePath)) {
					Directory.CreateDirectory(basePath);
				}
			}

			T asset = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(asset, $"Assets{Path.DirectorySeparatorChar}{genPath}");

			EditorApplication.delayCall += () => {
				SerializedObject serializedObject = new SerializedObject(
					AssetDatabase.LoadAssetAtPath<T>($"Assets{Path.DirectorySeparatorChar}{genPath}")
				);
				Edit(serializedObject);
				serializedObject.ApplyModifiedProperties();
			};
		}

		protected abstract void Edit(SerializedObject serializedObject);

		protected void SetAddress(SerializedObject serializedObject, string address) {
			string assetPath = AssetDatabase.GetAssetPath(serializedObject.targetObject);
			string guid = AssetDatabase.AssetPathToGUID(assetPath);
			
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
			
			AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
			entry.address = address;
		}
	}
}