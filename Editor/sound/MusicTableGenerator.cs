using hexegeer.internallib;
using UnityEditor;

namespace hexegeer.editor {
	internal sealed class MusicTableGenerator : ResourceGenerator<MusicTable> {
		protected override void Edit(SerializedObject serializedObject) {
			MusicSettings settings = MusicSettings.instance;

			SerializedProperty rowsProperty = serializedObject.FindProperty("_rows");

			rowsProperty.arraySize = settings.MusicList.Count;

			for (int i = 0; i < rowsProperty.arraySize; ++i) {
				SerializedProperty rowProperty = rowsProperty.Of(i);
				rowProperty.Of("id").intValue = settings.MusicList[i].id;
				rowProperty.Of("address").stringValue = settings.MusicList[i].address;
			}

			SetAddress(serializedObject, MusicTable.RESOURCE_ADDRESS);
		}
	}
}