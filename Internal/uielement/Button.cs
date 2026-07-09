using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public class ClickButton : CommonVisualElement<ClickButton> {
		private Label _label;

		public event System.Action OnClicked;

		private Color _color;
		private Color _hoverColor;
		private Color _clickColor;

		private Color _borderColor;
		private Color _borderHoverColor;
		private Color _borderClickColor;

		private Color _textColor;
		private Color _textHoverColor;
		private Color _textClickColor;

		private ClickButton(Align align) {
			style.backgroundColor = _color;
			style.alignContent = UnityEngine.UIElements.Align.Center;
			style.alignSelf = align;
			style.justifyContent = Justify.Center;
			style.minWidth = 50.0f;
			style.minHeight = 30.0f;
			style.flexGrow = 0f;

			RegisterCallback<GeometryChangedEvent>(
				evt => {
					float radius = Mathf.Min(resolvedStyle.width, resolvedStyle.height) * 0.5f;
					style.borderTopLeftRadius = radius;
					style.borderTopRightRadius = radius;
					style.borderBottomLeftRadius = radius;
					style.borderBottomRightRadius = radius;
				}
			);

			RegisterCallback<MouseDownEvent>(
				evt => {
					if (evt.button == 0) { 
						style.backgroundColor = _clickColor;
						style.color = _textClickColor;
						SetBorderColor(_borderClickColor);
						OnClicked?.Invoke();
					}
				}
			);
			RegisterCallback<MouseUpEvent>(
				evt => {
					if (evt.button == 0) {
						style.backgroundColor = _hoverColor;
						style.color = _textHoverColor;
						SetBorderColor(_borderHoverColor);
					}
				}
			);
			RegisterCallback<MouseEnterEvent>(
				evt => {
					style.backgroundColor = _hoverColor;
					style.color = _textHoverColor;
					SetBorderColor(_borderHoverColor);
				}
			);
			RegisterCallback<MouseLeaveEvent>(
				evt => {
					style.backgroundColor = _color;
					style.color = _textColor;
					SetBorderColor(_borderColor);
				}
			);

			_label = new Label("BUTTON");
			_label.style.unityTextAlign = TextAnchor.MiddleCenter;

			Add(_label);
		}

		public static ClickButton Create(Align align = UnityEngine.UIElements.Align.FlexStart) {
			return new ClickButton(align).Border(new Color(0.5f, 0.5f, 0.5f), 1f, 0f)
				.Padding(vertical: 6, horizontal: 16)
				.BackgroundColor(
					new Color(0.2f, 0.2f, 0.2f),
					new Color(0.25f, 0.25f, 0.25f),
					new Color(1.0f, 0.33f, 0.0f)
				).TextColor(
					new Color(0.8f, 0.8f, 0.8f),
					new Color(0.6f, 0.75f, 1.0f),
					new Color(1.0f, 1.0f, 1.0f)
				).BorderColor(
					new Color(0.4f, 0.4f, 0.4f),
					new Color(0.6f, 0.75f, 1.0f),
					new Color(0.4f, 0.4f, 0.4f)
				);
		}

		private void SetBorderColor(Color color) {
			style.borderLeftColor = color;
			style.borderRightColor = color;
			style.borderTopColor = color;
			style.borderBottomColor = color;
		}

		public ClickButton BackgroundColor(
			Color color,
			Color hoverColor,
			Color clickedColor
		) {
			_color = color;
			_hoverColor = hoverColor;
			_clickColor = clickedColor;
			return this;
		}

		public ClickButton BorderColor(
			Color color,
			Color hoverColor,
			Color clickedColor
		) {
			_borderColor = color;
			_borderHoverColor = hoverColor;
			_borderClickColor = clickedColor;
			return this;
		}

		public ClickButton TextColor(
			Color color,
			Color hoverColor,
			Color clickedColor
		) {
			_textColor = color;
			_textHoverColor = hoverColor;
			_textClickColor = clickedColor;
			return this;
		}

		public ClickButton Label(string text) {
			_label.text = text;
			return this;
		}

		public ClickButton IgnoreMinSize(bool width = true, bool height = true) {
			if (width) { 
				style.minWidth = 0f; 
				Padding(horizontal: 0f);
			}
			if (height) { 
				style.minHeight = 0f; 
				Padding(vertical: 0f);
			}
			return this;
		}

		public ClickButton Circle(float size = 30f) {
			return Width(size).Height(size).IgnoreMinSize();
		}
	}
}