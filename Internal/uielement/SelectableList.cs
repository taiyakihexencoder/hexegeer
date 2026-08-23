using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	/// <summary>
	/// 選択可能なアイテムリスト
	/// </summary>
	public class SelectableList<T> : CommonVisualElement<SelectableList<T>> where T: class {
		private ScrollView _scrollView;
		public T Selected { get; private set; }

		private Color _selectedColor;

		public System.Action<T> selectionChanged;

		public SelectableList() : base() {
			_scrollView = new ScrollView(ScrollViewMode.Vertical);
			_scrollView.style.flexDirection = FlexDirection.Column;
			_scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
			Add(_scrollView);
		}

		/// <summary>
		/// 色の設定
		/// </summary>
		/// <param name="selectedColor"></param>
		/// <returns></returns>
		public SelectableList<T> Colors(Color selectedColor) {
			_selectedColor = selectedColor;
			return this;
		}

		/// <summary>
		/// 選択
		/// </summary>
		/// <param name="key"></param>
		public void Select(T key) {
			if (!key.Equals(Selected)) {
				foreach(VisualElement ve in _scrollView.Children()) {
					if (ve.userData is T userData && userData.Equals(key)) {
						Selected = key;
						OnListSelected();
						selectionChanged?.Invoke(userData);
					}
				}
			}
		}

		/// <summary>
		/// 選択肢の追加
		/// </summary>
		/// <param name="key"></param>
		/// <param name="ve"></param>
		public void AddSelection(T key, VisualElement ve) {
			ve.userData = key;
			ve.style.width = new StyleLength(StyleKeyword.Auto);
			ve.RegisterCallback<MouseDownEvent>(
				evt => {
					if(evt.button == 0) {
						if (ve.userData is T userData && !userData.Equals(Selected)) {
							Selected = userData;
							OnListSelected();
							selectionChanged?.Invoke(userData);
						}
					}
				}
			);
			_scrollView.Add(ve);
		}

		private void OnListSelected() {
			foreach(VisualElement ve in _scrollView.Children()) {
				if (ve.userData is T userData) {
					Color color = _selectedColor;
					color.a = userData.Equals(Selected) ? 1.0f : 0.0f;
					ve.style.backgroundColor = new StyleColor(color);
				}
			}
		}

		/// <summary>
		/// 選択解除
		/// </summary>
		public void Unselect() {
			Selected = null;
			OnListSelected();
			selectionChanged?.Invoke(null);
		}

		/// <summary>
		/// 選択肢の削除
		/// </summary>
		public void ClearElements() {
			_scrollView.Clear();
			Unselect();
		}
	}

	/// <summary>
	/// 選択可能なアイテムリスト
	/// </summary>
	public class SelectableEnumList<T> : CommonVisualElement<SelectableEnumList<T>> where T: System.Enum {
		private ScrollView _scrollView;
		public T Selected { get; private set; }

		private Color _selectedColor;

		public System.Action<T> selectionChanged;

		public SelectableEnumList() : base() {
			_scrollView = new ScrollView(ScrollViewMode.Vertical);
			_scrollView.style.flexDirection = FlexDirection.Column;
			_scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
			Add(_scrollView);

			foreach (T entry in System.Enum.GetValues(typeof(T))) {
				AddSelection(
					entry, 
					Text.Body(entry.ToString())
						.TextAlign(TextAnchor.MiddleCenter)
						.WidthPercent(100f)
						.Height(40f)
				);
			}
		}

		/// <summary>
		/// 色の設定
		/// </summary>
		/// <param name="selectedColor"></param>
		/// <returns></returns>
		public SelectableEnumList<T> Colors(Color selectedColor) {
			_selectedColor = selectedColor;
			return this;
		}

		/// <summary>
		/// 選択
		/// </summary>
		/// <param name="key"></param>
		public void Select(T key) {
			foreach(VisualElement ve in _scrollView.Children()) {
				if (ve.userData is T userData && userData.Equals(key)) {
					Selected = key;
					OnListSelected();
					selectionChanged?.Invoke(userData);
				}
			}
		}

		/// <summary>
		/// 選択肢の追加
		/// </summary>
		/// <param name="key"></param>
		/// <param name="ve"></param>
		private void AddSelection(T key, VisualElement ve) {
			ve.userData = key;
			ve.style.width = new StyleLength(StyleKeyword.Auto);
			ve.RegisterCallback<MouseDownEvent>(
				evt => {
					if(evt.button == 0) {
						if (ve.userData is T userData && !userData.Equals(Selected)) {
							Selected = userData;
							OnListSelected();
							selectionChanged?.Invoke(userData);
						}
					}
				}
			);
			_scrollView.Add(ve);
		}

		private void OnListSelected() {
			foreach(VisualElement ve in _scrollView.Children()) {
				if (ve.userData is T userData) {
					Color color = _selectedColor;
					color.a = userData.Equals(Selected) ? 1.0f : 0.0f;
					ve.style.backgroundColor = new StyleColor(color);
				}
			}
		}
	}
}
