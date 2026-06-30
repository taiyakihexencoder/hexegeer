using UnityEngine.UIElements;

namespace hexegeer.internallib {
	/// <summary>
	///  Column Layout
	/// </summary>
	public class Column : CommonVisualElement<Column> {
		public Column() : base() {
			style.flexDirection = FlexDirection.Column;
		}

		public Column VerticalArrangement(Justify arrangement) {
			style.justifyContent = arrangement;
			return this;
		}

		public Column HorizontalAlignment(Align align) {
			return Align(align);
		}
	}
}