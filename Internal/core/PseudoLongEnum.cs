using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace hexegeer.internallib {
	/// <summary>
	/// 継承先で_id, _nameを定義してId, Name Getterを定義する。
	/// classだとECSから利用しづらいのでstructで定義できるようにした。
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IPseudoLongEnum<T> : IPseudoLongEnum, System.IComparable<T> where T : IPseudoLongEnum<T> {
		public static IEnumerable<T> GetAll() {
			System.Reflection.FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			T[] values = new T[fields.Length];
			for(int i = 0; i < fields.Length; ++i) {
				values[i] = (T)fields[i].GetValue(null);
			}
			return values;
		}

		int System.IComparable<T>.CompareTo(T other) { return Id.CompareTo(other.Id); }
		bool Equals(object obj) { return obj is T other && other.Id == Id; }
		int GetHashCode() { return Id.GetHashCode(); }
		string ToString() { return Name; }
	}

	public interface IPseudoLongEnum {
		long Id { get; }
		string Name { get; }
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(IPseudoLongEnum), true)]
	public sealed class PseudoLongEnumPropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			System.Type type = fieldInfo.FieldType;
			System.Reflection.FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			long[] ids = new long[fields.Length];
			GUIContent[] names = new GUIContent[fields.Length];
			for(int i = 0; i < fields.Length; ++i) {
				IPseudoLongEnum value = (IPseudoLongEnum) fields[i].GetValue(null);
				ids[i] = value.Id;
				names[i] = new GUIContent(value.Name);
			}

			SerializedProperty idProperty = property.FindPropertyRelative("_id");
			SerializedProperty nameProperty = property.FindPropertyRelative("_name");
			long selected = idProperty.longValue;
			int selectedIndex = -1;
			for (int i = 0; i < ids.Length; ++i) {
				if (selected == ids[i]) {
					selectedIndex = i;
					break;
				}
			}

			Vector2 labelSize = EditorStyles.label.CalcSize(label);
			EditorGUIUtility.labelWidth = labelSize.x;
			selectedIndex = EditorGUI.Popup(position, label, selectedIndex, names);
			
			if (selectedIndex < 0) {
				idProperty.longValue = 0;
				nameProperty.stringValue = "";
			} else {
				idProperty.longValue = ids[selectedIndex];
				nameProperty.stringValue = names[selectedIndex].text;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight;
		}
	}
#endif
}