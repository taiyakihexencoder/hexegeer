using hexegeer.internallib;
using UnityEditor;

namespace hexegeer.editor {
	internal sealed class EnvironmentSoundTableGenerator : ResourceGenerator<EnvironmentSoundTable> {
		protected override void Edit(SerializedObject serializedObject) {
			SoundSettings settings = SoundSettings.instance;

			SerializedProperty rowsProperty = serializedObject.FindProperty("_rows");

			rowsProperty.arraySize = settings.EnvironmentSoundList.Count;

			for (int i = 0; i < rowsProperty.arraySize; ++i) {
				SerializedProperty rowProperty = rowsProperty.Of(i);
				rowProperty.Of("id").intValue = settings.EnvironmentSoundList[i].id;
				rowProperty.Of("address").stringValue = settings.EnvironmentSoundList[i].address;
			}

			SetAddress(serializedObject, EnvironmentSoundTable.RESOURCE_ADDRESS);
		}
	}
}