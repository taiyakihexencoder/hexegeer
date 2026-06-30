using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public abstract class CommonVisualElement<T> : VisualElement 
		where T : CommonVisualElement<T> {
		protected T Self => this as T;

		public CommonVisualElement() {
			style.marginLeft = 0f;
			style.marginRight = 0f;
			style.marginTop = 0f;
			style.marginBottom = 0f;
			style.paddingLeft = 0f;
			style.paddingRight = 0f;
			style.paddingTop = 0f;
			style.paddingBottom = 0f;
			style.minWidth = 0f;
			style.minHeight = 0f;
		}

		public T Align(Align align) {
			style.alignContent = align;
			return Self;
		}

		public T Margin(float all) {
			style.marginLeft = all;
			style.marginRight = all;
			style.marginTop = all;
			style.marginBottom = all;
			return Self;
		}

		public T Margin(float vertical = 0f, float horizontal = 0f) {
			style.marginLeft = horizontal;
			style.marginRight = horizontal;
			style.marginTop = vertical;
			style.marginBottom = vertical;
			return Self;
		}

		public T Margin(float left = 0f, float right = 0f, float top = 0f, float bottom = 0f) {
			style.marginLeft = left;
			style.marginRight = right;
			style.marginTop = top;
			style.marginBottom = bottom;
			return Self;
		}

		public T Padding(float all) {
			style.paddingLeft = all;
			style.paddingRight = all;
			style.paddingTop = all;
			style.paddingBottom = all;
			return Self;
		}

		public T Padding(float vertical = 0f, float horizontal = 0f) {
			style.paddingLeft = horizontal;
			style.paddingRight = horizontal;
			style.paddingTop = vertical;
			style.paddingBottom = vertical;
			return Self;
		}

		public T Padding(float left = 0f, float right = 0f, float top = 0f, float bottom = 0f) {
			style.paddingLeft = left;
			style.paddingRight = right;
			style.paddingTop = top;
			style.paddingBottom = bottom;
			return Self;
		}

		public T Background(Color color) {
			style.backgroundColor = color;
			return Self;
		}

		public T Border(Color color, float width = 1f, float radius = 0f) {
			style.borderLeftWidth = width;
			style.borderRightWidth = width;
			style.borderTopWidth = width;
			style.borderBottomWidth = width;

			style.borderLeftColor = color;
			style.borderRightColor = color;
			style.borderTopColor = color;
			style.borderBottomColor = color;

			style.borderTopLeftRadius = radius;
			style.borderTopRightRadius = radius;
			style.borderBottomLeftRadius = radius;
			style.borderBottomRightRadius = radius;
			return Self;
		}

		public T Width(float width) {
			style.width = width;
			return Self;
		}

		public T WidthPercent(float percent) {
			style.width = new StyleLength(new Length(percent, LengthUnit.Percent));
			return Self;
		}

		public T Height(float height) {
			style.height = height;
			return Self;
		}

		public T HeightPercent(float percent) {
			style.height = new StyleLength(new Length(percent, LengthUnit.Percent));
			return Self;
		}

		public T Weight(float weight) {
			style.flexBasis = 0f;
			style.flexGrow = weight;
			return Self;
		}

		public T AddChildren(params VisualElement[] children) {
			foreach(VisualElement child in children) {
				Add(child);
			}
			return Self;
		}
	}
}