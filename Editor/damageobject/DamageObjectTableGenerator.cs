using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;

namespace hexegeer.editor {
	internal sealed class DamageObjectTableGenerator : ResourceGenerator<DamageObjectTable> {
		protected override void Edit(SerializedObject serializedObject) {
			DamageObjectSettings settings = DamageObjectSettings.instance;
			DamageObjectColliderSettings colliderSettings = DamageObjectColliderSettings.instance;
			ContentKeySetting contentKeySettings = ContentKeySetting.instance;
			LayerSettings layerSettings = LayerSettings.instance;
			Dictionary<int, int> layerTable = new Dictionary<int, int>();
			foreach(int a in layerSettings.LayerIndices) {
				int flags = 0;
				foreach(int b in layerSettings.LayerIndices) {
					if (layerSettings.Table(a, b)) {
						flags |= (1 << b);
					}
				}
				layerTable.Add(a, flags);
			}

			SerializedProperty damageObjectsProperty = serializedObject.FindProperty("_damageObjects");
			damageObjectsProperty.arraySize = settings.Rows.Count;

			SerializedProperty collidersProperty = serializedObject.FindProperty("_colliders");
			collidersProperty.arraySize = colliderSettings.Colliders.Count;

			SerializedProperty contentKeyTableProperty = serializedObject.FindProperty("_contentKeyTable");
			contentKeyTableProperty.arraySize = contentKeySettings.Keys.Count + 1;
			Dictionary<int, int> contentKeyIdToIndex = new Dictionary<int, int>();
			contentKeyTableProperty.Of(0).Of("key").intValue = ContentKey.Global.value;
			contentKeyTableProperty.Of(0).Of("indices").arraySize = 0;
			contentKeyIdToIndex.Add(ContentKey.Global.value, 0);
			for (int i = 0; i < contentKeySettings.Keys.Count; ++i) {
				SerializedProperty contentKeyProperty = contentKeyTableProperty.Of(i+1);
				contentKeyProperty.Of("key").intValue = contentKeySettings.Keys[i].id;
				contentKeyProperty.Of("indices").arraySize = 0;
				contentKeyIdToIndex.Add(contentKeySettings.Keys[i].id, i+1);
			}

			for (int i = 0; i < settings.Rows.Count; ++i) {
				SerializedProperty damageObjectProperty = damageObjectsProperty.Of(i);
				damageObjectProperty.Of("id").intValue = settings.Rows[i].id;
				damageObjectProperty.Of("name").stringValue = settings.Rows[i].name;
				damageObjectProperty.Of("asset").stringValue = settings.Rows[i].asset;
				damageObjectProperty.Of("collider").intValue = settings.Rows[i].collider;
				foreach (int contentKey in settings.Rows[i].contentKeys) {
					int index = contentKeyIdToIndex[contentKey];
					SerializedProperty indicesProperty = contentKeyTableProperty.Of(index).Of("indices");
					indicesProperty.Add(p => p.intValue = i);
				}
			}

			for (int i = 0; i < colliderSettings.Colliders.Count; ++i) {
				SerializedProperty colliderProperty = collidersProperty.Of(i);
				colliderProperty.Of("id").intValue = colliderSettings.Colliders[i].id;
				colliderProperty.Of("name").stringValue = colliderSettings.Colliders[i].name;
				colliderProperty.Of("belongsTo").intValue = 1 << colliderSettings.Colliders[i].layer;
				colliderProperty.Of("collidesWith").intValue = layerTable.TryGetValue(colliderSettings.Colliders[i].layer, out int flags) ? flags : 0;
				colliderProperty.Of("shape").intValue = (int) colliderSettings.Colliders[i].shape;
				colliderProperty.Of("extent").vector3Value = colliderSettings.Colliders[i].extent;
				colliderProperty.Of("rotation").quaternionValue = colliderSettings.Colliders[i].rotation;
			}

			SetAddress(serializedObject, DamageObjectTable.RESOURCE_ADDRESS);
		}
	}
}