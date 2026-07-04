using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	internal sealed class FieldTableGenerator : ResourceGenerator<FieldTable> {
		protected override void Edit(SerializedObject serializedObject) {
			FieldMainSettings mainSettings = FieldMainSettings.instance;
			System.Type resourceType = mainSettings.ViewType.GetResourceType();

			SerializedProperty rowsProperty = serializedObject.FindProperty("_rows");

			foreach (string guid in AssetDatabase.FindAssets($"t:{resourceType.Name}")) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				BaseFieldBlueprint blueprint = AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath);

				// フィールドの領域範囲
				Vector3 regionMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				Vector3 regionMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

				for (int i = 0; i < blueprint.MeshCount; ++i) {
					if (blueprint.TryGetMesh(i, out Vector3[] vertices, out int[] indices)) {
						foreach(Vector3 v in vertices) {
							Vector3 pos = blueprint.Rotation * v;
							if (pos.x < regionMin.x) { regionMin.x = pos.x; } else if (pos.x > regionMax.x) { regionMax.x = pos.x; }
							if (pos.y < regionMin.y) { regionMin.y = pos.y; } else if (pos.y > regionMax.y) { regionMax.y = pos.y; }
							if (pos.z < regionMin.z) { regionMin.z = pos.z; } else if (pos.z > regionMax.z) { regionMax.z = pos.z; }
						}
					}
				}

				if (regionMin.x > regionMax.x || regionMin.y > regionMax.y || regionMin.z > regionMax.z) {
					regionMin = Vector3.zero;
					regionMax = Vector3.zero;
				}

				rowsProperty.Add( p => {
					p.FindPropertyRelative("id").intValue = blueprint.Id;
					p.FindPropertyRelative("contentKey").intValue = blueprint.ContentKey;
					p.FindPropertyRelative("address").stringValue = blueprint.RuntimeAssetAddress;
					p.FindPropertyRelative("name").stringValue = blueprint.name;
					p.FindPropertyRelative("guid").stringValue = guid;
					p.FindPropertyRelative("position").vector3Value = blueprint.Position;
					p.FindPropertyRelative("rotation").quaternionValue = blueprint.Rotation;
					p.FindPropertyRelative("boundsMin").vector3Value = regionMin + blueprint.Position;
					p.FindPropertyRelative("boundsMax").vector3Value = regionMax + blueprint.Position;
				});
			}

			SetAddress(serializedObject, FieldTable.RESOURCE_ADDRESS);
		}
	}
}