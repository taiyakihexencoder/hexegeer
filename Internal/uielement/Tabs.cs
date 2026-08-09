using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public sealed class Tabs : CommonVisualElement<Tabs> {
		struct TabEntity {
			public string text;
		}

		private int _selectedIndex;
		public int SelectedIndex => _selectedIndex;
		public event System.Action<int> SelectedIndexChanged;

		private List<TabEntity> _tabEntities;

		private Color _selectedForegroundColor;
		private Color _defaultForegroundColor;

		private Color _selectedBackgroundColor;
		private Color _defaultBackgroundColor;

		private bool _pressedLeftMouseButton = false;
		private float _tabScroll = 0.0f;
		private bool _scrolled = false;

		public Tabs(int selectedIndex = -1) : base() {
			_selectedForegroundColor = new Color(0f, 0f, 0f, 1f);
			_defaultForegroundColor = new Color(1f, 1f, 1f, 1f);

			_selectedBackgroundColor = new Color(1f, 0.7f, 0.3f, 1f);
			_defaultBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);

			_tabEntities = new List<TabEntity>();
			_selectedIndex = selectedIndex;

			style.flexDirection = FlexDirection.Row;
		}

		public void Add(params string[] entries) {
			foreach(string entry in entries) {
				_tabEntities.Add(
					new TabEntity {
						text = entry,
					}
				);
			}
			UpdateView();
		}

		public void SetSelectedIndex(int index, bool updateView = true) {
			if (index != _selectedIndex) {
				_selectedIndex = index;
				if (updateView) {
					SelectedIndexChanged(_selectedIndex);
					UpdateView();
				}
			}
		}

		private void UpdateView() {
			Clear();
			VisualElement tabView = new VisualElement();
			Add(tabView);
			tabView.style.flexDirection = FlexDirection.Row;
			tabView.style.flexGrow = 1f;
			tabView.style.height = 24f;
			tabView.style.translate = new Translate(_tabScroll, 0.0f);

			for (int i = 0; i < _tabEntities.Count; ++i) {
				bool selected = i == _selectedIndex;
				TabElement tab = new TabElement(
					_tabEntities[i],
					index: i,
					onClick: onClickElement
				);
				tab.style.backgroundColor = selected ? _selectedBackgroundColor : _defaultBackgroundColor;
				tab.Q<Label>().style.color = selected ? _selectedForegroundColor : _defaultForegroundColor;
				tabView.Add(tab);
			}

			// 各タブの幅を合わせる
			tabView.RegisterCallback<GeometryChangedEvent>(evt => ResizeTab(tabView));
			tabView.RegisterCallback<AttachToPanelEvent>(evt => ResizeTab(tabView));

			this.RegisterCallback<MouseDownEvent>(evt => {
				if (evt.button == 0) {
					_pressedLeftMouseButton = true;
					_scrolled = false;
				}
			});
			this.RegisterCallback<MouseUpEvent>(evt => {
				if (evt.button == 0) {
					_pressedLeftMouseButton = false;
					if (_scrolled) {
						evt.StopPropagation();
					}
				}
			}, TrickleDown.TrickleDown);
			this.RegisterCallback<MouseLeaveEvent>(evt => {
				_pressedLeftMouseButton = false;
			});
			this.RegisterCallback<MouseMoveEvent>(evt => {
				float viewWidth = resolvedStyle.width;
				float tabWidth = tabView.resolvedStyle.width;
				if (_pressedLeftMouseButton && viewWidth < tabWidth) {
					_scrolled = true;
					float delta = evt.mouseDelta.x;
					_tabScroll = Mathf.Clamp(_tabScroll + delta, viewWidth - tabWidth, 0.0f);
					tabView.style.translate = new Translate(_tabScroll, 0.0f);
				}
			});
		}

		private void ResizeTab(VisualElement tabView) {
			int n = 0;
			float maxTextWidth = 0f;
			{
				this.Query<TabElement>().ForEach(tab => {
					float textWidth = tab.Q<Label>().MeasureTextSize(_tabEntities[n].text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined).x;
					if (maxTextWidth < textWidth) {
						maxTextWidth = textWidth;
					}
					n++;
				});
			}

			// リストビューの領域サイズよりも小さくなるなら広げ、大きくなるなら固定値にする
			float preferWidth = maxTextWidth + 18f;
			tabView.style.minWidth = preferWidth * n;
			this.Query<TabElement>().ForEach(tab => {
				tab.style.minWidth = preferWidth;
				tab.style.flexBasis = 0f;
				tab.style.flexGrow = 1f;
			});
		}

		private void onClickElement(int index) {
			if (_selectedIndex == index) {
				_selectedIndex = -1;
			} else {
				_selectedIndex = index;
			}
			SelectedIndexChanged(_selectedIndex);
			UpdateView();
		}

		private class TabElement : VisualElement {
			public TabElement(
				TabEntity entity,
				int index,
				System.Action<int> onClick
			) {
				style.justifyContent = Justify.Center;
				style.flexShrink = 0f;

				Color borderColor = Color.black;
				style.borderTopColor = borderColor;
				style.borderBottomColor = borderColor;
				style.borderLeftColor = borderColor;
				style.borderRightColor = borderColor;

				float borderWidth = 1f;
				style.borderTopWidth = borderWidth;
				style.borderBottomWidth = borderWidth;
				style.borderLeftWidth = borderWidth;
				style.borderRightWidth = borderWidth;
				style.paddingLeft = 8f;
				style.paddingRight = 8f;
				style.paddingTop = 6f;
				style.paddingBottom = 4f;
				Label label = new Label(entity.text);
				label.style.alignSelf = UnityEngine.UIElements.Align.Center;
				Add(label);

				RegisterCallback<MouseUpEvent>(
					evt => {
						if (evt.button == 0) {
							onClick(index);
						}
					}
				);
			}
		}
	}
}