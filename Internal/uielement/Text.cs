using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public class Text : CommonVisualElement<Text> {
		private Label label;

		public string text {
			get => label.text;
			set => label.text = value;
		}

		private Text(string text) {
			label = new Label(text);
			Add(label);
		}

		public static Text H1(string text) {
			return new Text(text)
				.FontSize(28)
				.Style(FontStyle.Bold)
				.Padding(vertical:7);
		}

		public static Text H2(string text) {
			return new Text(text)
				.FontSize(20)
				.Style(FontStyle.Bold)
				.Padding(vertical:5);
		}

		public static Text H3(string text) {
			return new Text(text)
				.FontSize(16)
				.Style(FontStyle.Bold)
				.Padding(vertical:4);
		}

		public static Text Body(string text) {
			return new Text(text);
		}

		public Text Bold() {
			return Style(FontStyle.Bold);
		}

		public Text FontSize(float fontSize) {
			style.fontSize = fontSize;
			return this;
		}

		public Text Style(FontStyle fontStyle) {
			style.unityFontStyleAndWeight = fontStyle;
			return this;
		}
	}
}