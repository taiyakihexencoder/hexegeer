using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public class ScrollPane : CommonVisualElement<ScrollPane> {
		private ScrollView _scrollView;

		public ScrollPane() {
			style.width = new StyleLength(new Length(100, LengthUnit.Percent));
			style.height = new StyleLength(new Length(100, LengthUnit.Percent));

			_scrollView = new ScrollView(ScrollViewMode.Vertical);
			_scrollView.style.flexDirection = FlexDirection.Column;
			_scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
			base.Add(_scrollView);
		}

		public override ScrollPane AddChildren(params VisualElement[] children) {
			foreach(VisualElement child in children) {
				_scrollView.Add(child);
			}
			return this;
		}

		public new void Add(VisualElement element) {
			_scrollView.Add(element);
		}

		public new void Insert(int index, VisualElement element) {
			_scrollView.Insert(index, element);
		}

		public new void Clear() {
			_scrollView.Clear();
		}
	}
}
