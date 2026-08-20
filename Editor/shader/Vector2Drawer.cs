using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class Vector2Drawer : MaterialPropertyDrawer {
		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) {
			using (EditorGUI.ChangeCheckScope scope = new EditorGUI.ChangeCheckScope()) {
				EditorGUIUtility.labelWidth /= 3;
				Vector2 value = EditorGUI.Vector2Field(position, label, new Vector2(prop.vectorValue.x, prop.vectorValue.y));

				if (scope.changed) {
					prop.vectorValue = new Vector4(value.x, value.y, 0f, 0f);
				}
			}
		}
	}
}