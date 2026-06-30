using UnityEngine.UIElements;

namespace hexegeer.internallib {
	/// <summary>
	/// Row Layout
	/// </summary>
	public class Row : CommonVisualElement<Row> {
		public Row() : base() {
			style.flexDirection = FlexDirection.Row;
		}

		public Row HorzontalArrangement(Justify arrangement) {
			style.justifyContent = arrangement;
			return this;
		}

		public Row VerticalAlignment(Align align) {
			return Align(align);
		}
	}
}
