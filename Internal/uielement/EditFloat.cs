using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public class EditFloat : CommonVisualElement<EditFloat> {
		private FloatField _field;
		private Label _label;

		public float Value {
			get => _field.value;
			set => _field.value = value;
		}

		public EditFloat(float value = 0.0f) {
			style.flexDirection = FlexDirection.Row;
			_field = new FloatField();
			_field.SetValueWithoutNotify(value);
			_field.style.flexGrow = 3f;
			_field.style.flexBasis = 0f;
			Add(_field);
		}

		public EditFloat Label(string text) {
			if (_label == null) {
				_label = new Label();
				_label.style.maxWidth = 200f;
				_label.style.unityTextAlign = TextAnchor.MiddleLeft;
				_label.style.paddingRight = 12;
				_label.style.flexGrow = 1f;
				_label.style.flexBasis = 0f;
				Insert(0, _label);
			}
			_label.text = text;
			return this;
		}

		public EditFloat OnChanged(System.Action<float> onChanged) {
			_field.RegisterValueChangedCallback( v => {
				onChanged(v.newValue);
			});
			return this;
		}
	}
}