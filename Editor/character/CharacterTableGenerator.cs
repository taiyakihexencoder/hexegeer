using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;

namespace hexegeer.editor {
	internal sealed class CharacterTableGenerator : ResourceGenerator<CharacterTable> {
		protected override void Edit(SerializedObject serializedObject) {
			CharacterSettings settings = CharacterSettings.instance;
			CharacterColliderSettings colliderSettings = CharacterColliderSettings.instance;
			ContentKeySetting contentKeySettings = ContentKeySetting.instance;
			LayerSettings layerSettings = LayerSettings.instance;

			SerializedProperty charactersProperty = serializedObject.FindProperty("_characters");
			charactersProperty.arraySize = settings.Characters.Count;

			for (int i = 0; i < charactersProperty.arraySize; ++i) {
				SerializedProperty characterProperty = charactersProperty.Of(i);
				CharacterSettings.CharacterData data = settings.Characters[i];
				int collidesWith = 0;
				for (int j = 0; j < LayerSettings.LAYER_COUNT; ++j) {
					if (layerSettings.Table(j, data.layer)) {
						collidesWith |= 1 << j;
					}
				}

				characterProperty.Of("id").intValue = data.id;
				characterProperty.Of("name").stringValue = data.name;
				characterProperty.Of("collider").intValue = data.collider;
				characterProperty.Of("belongsTo").intValue = 1 << data.layer;
				characterProperty.Of("collidesWith").intValue = collidesWith;
				characterProperty.Of("hasObservationPoint").boolValue = settings.IsObservationPoint(data);

				// model and animations
				SerializedProperty modelProfileProperty = characterProperty.Of("modelProfile");
				modelProfileProperty.Of("modelAsset").stringValue = data.modelProfile.modelAsset;
				
				SerializedProperty overrideAnimationsProperty = modelProfileProperty.Of("overrideAnimations");
				overrideAnimationsProperty.arraySize = data.modelProfile.overrideAnimations.Count;
				for (int j = 0; j < data.modelProfile.overrideAnimations.Count; ++j) {
					overrideAnimationsProperty.Of(j).stringValue = data.modelProfile.overrideAnimations[j];
				}

				SerializedProperty additiveAnimationsProperty = modelProfileProperty.Of("additiveAnimations");
				additiveAnimationsProperty.arraySize = data.modelProfile.additiveAnimations.Count;
				for (int j = 0; j < data.modelProfile.additiveAnimations.Count; ++j) {
					additiveAnimationsProperty.Of(j).stringValue = data.modelProfile.additiveAnimations[j];
				}

				SerializedProperty baseAnimationsProperty = modelProfileProperty.Of("baseAnimations");
				baseAnimationsProperty.arraySize = data.modelProfile.baseAnimations.Count;
				for (int j = 0; j < data.modelProfile.baseAnimations.Count; ++j) {
					baseAnimationsProperty.Of(j).stringValue = data.modelProfile.baseAnimations[j];
				}
			}

			SerializedProperty collidersProperty = serializedObject.FindProperty("_colliders");
			collidersProperty.arraySize = colliderSettings.PhysicsColliders.Count;
			for (int i = 0; i < collidersProperty.arraySize; ++i) {
				SerializedProperty colliderProperty = collidersProperty.Of(i);
				CharacterColliderSettings.PhysicsCollider data = colliderSettings.PhysicsColliders[i];
				colliderProperty.Of("id").intValue = data.id;
				colliderProperty.Of("name").stringValue = data.name;
				colliderProperty.Of("radius").floatValue = data.radius;
				colliderProperty.Of("height").floatValue = data.height;
			}

			SerializedProperty keyTablesProperty = serializedObject.FindProperty("_keyTables");
			keyTablesProperty.arraySize = contentKeySettings.Keys.Count + 1;
			keyTablesProperty.Of(0).Of("key").intValue = ContentKey.Global.value;
			keyTablesProperty.Of(0).Of("characterIndices").arraySize = 0;
			for (int i = 1; i < keyTablesProperty.arraySize; ++i) {
				SerializedProperty keyTableProperty = keyTablesProperty.Of(i);
				ContentKeySetting.Key data = contentKeySettings.Keys[i-1];
				keyTableProperty.Of("key").intValue = data.id;
				keyTableProperty.Of("characterIndices").arraySize = 0;
			}

			List<CharacterSettings.CharacterData> characters = settings.Characters;
			List<ContentKeySetting.Key> keys = new List<ContentKeySetting.Key>(contentKeySettings.Keys);
			for(int i = 0; i < characters.Count; ++i) {
				foreach(int key in characters[i].contentKeys) {
					if (key == ContentKey.Global.value) {
						keyTablesProperty.Of(0).Of("characterIndices").Add(p => p.intValue = i);
					} else {
						int index = keys.FindIndex(_ => _.id == key);
						if (index >= 0) {
							SerializedProperty keyTableProperty = keyTablesProperty.Of(index+1);
							keyTableProperty.Of("characterIndices").Add(p => p.intValue = i);
						}
					}
				}
			}

			// Physics Object Layer
			// (interrnallibから参照できないため)
			SerializedProperty physicsObjectLayerProperty = serializedObject.FindProperty("_physicsObjectLayer");
			SerializedProperty physicsObjectCollidesProperty = serializedObject.FindProperty("_physicsObjectCollides");
			int layerIndex = layerSettings.LayerIndex((int)DefaultLayer.PhysicsObject);
			int collides = 0;
			physicsObjectLayerProperty.intValue = 1 << layerIndex;
			for (int i = 0; i < LayerSettings.LAYER_COUNT; ++i) {
				if (layerSettings.Table(i, layerIndex)) {
					collides |= (1 << i);
				}
			}
			physicsObjectCollidesProperty.intValue = collides;

			SetAddress(serializedObject, CharacterTable.RESOURCE_ADDRESS);
		}
	}
}