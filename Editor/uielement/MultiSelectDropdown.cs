using System.Collections.Generic;
using hexegeer.internallib;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public class MultiSelectDropdown<T> : CommonVisualElement<MultiSelectDropdown<T>> {
		private Image _icon;

		private Dictionary<T, bool> _items;
		private System.Func<T, string> _converter;
		private Label _label;

		public event System.Action<T, bool> OnSelectionChanged;

		public MultiSelectDropdown() {
			style.flexDirection = FlexDirection.Row;
			style.paddingBottom = 6f;
			style.paddingTop = 6f;
			style.paddingLeft = 10f;
			style.paddingRight = 10f;
			style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

			float borderWidth = 1f;
			style.borderTopWidth = borderWidth;
			style.borderLeftWidth = borderWidth;
			style.borderRightWidth = borderWidth;
			style.borderBottomWidth = borderWidth;

			Color borderColor = new Color(0.4f, 0.4f, 0.4f);
			style.borderTopColor = borderColor;
			style.borderLeftColor = borderColor;
			style.borderRightColor = borderColor;
			style.borderBottomColor = borderColor;

			float radius = 2f;
			style.borderTopLeftRadius = radius;
			style.borderTopRightRadius = radius;
			style.borderBottomLeftRadius = radius;
			style.borderBottomRightRadius = radius;

			_label = new Label("( None )");
			_label.style.flexBasis = 0f;
			_label.style.flexGrow = 1f;
			_label.style.minWidth = 60f;
			_label.style.color = new Color(0.8f,0.8f,0.8f);
			Add(_label);

			_icon = new Image();
			_icon.style.width = 12;
			_icon.style.height = 12;
			_icon.image = EditorGUIUtility.IconContent("d_icon dropdown").image;
			_items = new Dictionary<T, bool>();
			Add(_icon);

			RegisterCallback<MouseUpEvent>(
				evt => {
					if (evt.button == 0) {
						GenericDropdownMenu menu = MakeDropdown();
						menu.DropDown(new Rect(worldBound.x, worldBound.yMax, 0, 0), _label, DropdownMenuSizeMode.Content);
					}
				}
			);
		}

		public MultiSelectDropdown<T> SetKeys(Dictionary<T, bool> items, System.Func<T, string> converter) {
			_items = new Dictionary<T, bool>(items);
			_converter = converter;

			List<string> list = GetSelectedItems();
			_label.text = list.Count > 0 ? string.Join(',', list) : "( None )";
			return this;
		}

		private GenericDropdownMenu MakeDropdown() {
			GenericDropdownMenu dropdown = new GenericDropdownMenu();
			foreach(T item in _items.Keys) {
				dropdown.AddItem(
					_converter(item), 
					_items[item], 
					() => {
						_items[item] = !_items[item];
						List<string> list = GetSelectedItems();
						_label.text = list.Count > 0 ? string.Join(',', list) : "( None )";
						OnSelectionChanged?.Invoke(item, _items[item]);
					}
				);
			}
			return dropdown;
		}

		private List<string> GetSelectedItems() {
			List<string> selected = new List<string>();
			foreach (T key in _items.Keys) {
				if (_items[key]) { selected.Add(_converter(key)); }
			}
			return selected;
		}
	}
}