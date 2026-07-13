using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace hexegeer.internallib {
	public abstract class PseudoEnum<T> : PseudoEnum, System.IComparable<T> where T: PseudoEnum<T> {
		[SerializeField]
		private int _id;
		public override int Id => _id;

		[SerializeField]
		private string _name;
		public override string Name => _name;

		public static IEnumerable<T> GetAll() {
			System.Reflection.FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			T[] values = new T[fields.Length];
			for(int i = 0; i < fields.Length; ++i) {
				values[i] = (T)fields[i].GetValue(null);
			}
			return values;
		} 

		protected PseudoEnum(int id, string name) {
			_id = id;
			_name = name;
		}

		int System.IComparable<T>.CompareTo(T other) {
			return _id.CompareTo(other._id);
		}

		public override int GetHashCode() {
			return _id.GetHashCode();
		}

		public override bool Equals(object obj) {
			return obj is T other && other._id == _id;
		}

		public override string ToString() {
			return _name;
		}
	}

	public abstract class PseudoEnum {
		public abstract int Id { get; }
		public abstract string Name { get; }
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(PseudoEnum<>), true)]
	public sealed class PseudoEnumPropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			System.Type type = fieldInfo.FieldType;
			System.Reflection.FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			int[] ids = new int[fields.Length];
			GUIContent[] names = new GUIContent[fields.Length];
			for(int i = 0; i < fields.Length; ++i) {
				PseudoEnum value = (PseudoEnum) fields[i].GetValue(null);
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