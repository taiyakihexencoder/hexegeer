using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class Vector3Drawer : MaterialPropertyDrawer {
		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
			using (EditorGUI.ChangeCheckScope scope = new EditorGUI.ChangeCheckScope()) {
				EditorGUIUtility.labelWidth /= 3;
				Vector3 value = EditorGUI.Vector3Field(position, label, new Vector3(prop.vectorValue.x, prop.vectorValue.y, prop.vectorValue.z));

				if (scope.changed) {
					prop.vectorValue = new Vector4(value.x, value.y, value.z, 0f);
				}
			}
		}
	}
}