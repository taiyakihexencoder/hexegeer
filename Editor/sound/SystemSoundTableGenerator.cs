using hexegeer.internallib;
using UnityEditor;

namespace hexegeer.editor {
	internal sealed class SystemSoundTableGenerator : ResourceGenerator<SystemSoundTable> {
		protected override void Edit(SerializedObject serializedObject) {
			SoundSettings settings = SoundSettings.instance;

			SerializedProperty rowsProperty = serializedObject.FindProperty("_rows");

			rowsProperty.arraySize = settings.SystemSoundList.Count;

			for (int i = 0; i < rowsProperty.arraySize; ++i) {
				SerializedProperty rowProperty = rowsProperty.Of(i);
				rowProperty.Of("id").intValue = settings.SystemSoundList[i].id;
				rowProperty.Of("address").stringValue = settings.SystemSoundList[i].address;
			}

			SetAddress(serializedObject, SystemSoundTable.RESOURCE_ADDRESS);
		}
	}
}