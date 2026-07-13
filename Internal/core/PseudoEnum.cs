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
	public interface IPseudoEnum<T> : IPseudoEnum, System.IComparable<T> where T : IPseudoEnum<T> {
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

	public interface IPseudoEnum {
		int Id { get; }
		string Name { get; }
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(IPseudoEnum), true)]
	public sealed class PseudoEnumPropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			System.Type type = fieldInfo.FieldType;
			System.Reflection.FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			int[] ids = new int[fields.Length];
			GUIContent[] names = new GUIContent[fields.Length];
			for(int i = 0; i < fields.Length; ++i) {
				IPseudoEnum value = (IPseudoEnum) fields[i].GetValue(null);
				ids[i] = value.Id;
				names[i] = new GUIContent(value.Name);
			}

			SerializedProperty idProperty = property.FindPropertyRelative("_id");
			int selectedIndex = idProperty.intValue;
			selectedIndex = EditorGUI.IntPopup(position, selectedIndex, names, ids);
			
			for (int i = 0; i < names.Length; ++i) {
				if (selectedIndex == ids[i]) {
					SerializedProperty nameProperty = property.FindPropertyRelative("_name");
					nameProperty.stringValue = names[i].text;
				}
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight;
		}
	}
#endif
}