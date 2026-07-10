using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	internal sealed class LayoutResourceGenerator : ResourceGenerator<LayoutTable> {

		protected override void Edit(SerializedObject serializedObject) {
			LayoutSetting setting = LayoutSetting.instance;

			Dictionary<int, (Vector3, Quaternion)> offsetTable = new Dictionary<int, (Vector3, Quaternion)>();
			FieldMainSettings fieldSettings = FieldMainSettings.instance;
			System.Type loadResourceType = fieldSettings.ViewType.GetResourceType();
			foreach(string guid in AssetDatabase.FindAssets($"t:{loadResourceType.Name}")) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				BaseFieldBlueprint bp = AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath);
				if (!offsetTable.ContainsKey(bp.ContentKey)) {
					offsetTable.Add(bp.ContentKey, (bp.Position, bp.Rotation));
				}
			}


			SerializedProperty layoutProfilesProperty = serializedObject.FindProperty("_layoutProfiles");
			layoutProfilesProperty.arraySize = setting.LayoutProfiles.Count;

			for (int i = 0; i < setting.LayoutProfiles.Count; ++i) {
				LayoutSetting.LayoutProfile profile = setting.LayoutProfiles[i];
				SerializedProperty layoutProfileProperty = layoutProfilesProperty.Of(i);

				// Content Key
				SerializedProperty contentKeyProperty = layoutProfileProperty.Of("_contentKey");
				contentKeyProperty.intValue = profile.contentKey;

				Vector3 offsetPosition = Vector3.zero;
				Quaternion offsetRotation = Quaternion.identity;
				if (offsetTable.TryGetValue(profile.contentKey, out (Vector3, Quaternion) value)) {
					offsetPosition = value.Item1;
					offsetRotation = value.Item2;
				}

				// Load Character Ids
				List<int> characterIds = profile.characters.Map(_ => _.character).Distinct();
				SerializedProperty characterIdsProperty = layoutProfileProperty.Of("_characterIds");
				characterIdsProperty.arraySize = characterIds.Count;
				for (int j = 0; j < characterIds.Count; ++j) {
					characterIdsProperty.Of(j).intValue = characterIds[j];
				}

				// Character Layouts
				SerializedProperty charactersProperty = layoutProfileProperty.Of("_characters");
				charactersProperty.arraySize = profile.characters.Count;
				for (int j = 0; j < profile.characters.Count; ++j) {
					LayoutSetting.CharacterLayout character = profile.characters[j];
					SerializedProperty characterProperty = charactersProperty.Of(j);
					characterProperty.Of("_id").intValue = character.character;
					characterProperty.Of("_position").vector3Value = offsetPosition + offsetRotation * character.position;
					characterProperty.Of("_rotation").quaternionValue = offsetRotation * character.rotation;
				}

			}
			SetAddress(serializedObject, LayoutTable.RESOURCE_ADDRESS);
		}
	}
}