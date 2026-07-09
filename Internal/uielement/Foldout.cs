using UnityEditor;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public class Foldout : CommonVisualElement<Foldout> {
		public bool Expanded {
			get {
				return _content.style.display == DisplayStyle.Flex;
			}
			set {
				_icon.image = EditorGUIUtility.IconContent(value ? "IN foldout on" : "IN foldout").image;
				_content.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		private Column _content;
		private Image _icon;

		public Foldout(VisualElement header, bool expanded = false) {
			Row row = new Row()
				.VerticalAlignment(UnityEngine.UIElements.Align.Center);
			row.RegisterCallback<MouseDownEvent>(evt => {
				if (evt.button == 0) {
					Expanded = !Expanded;
				}
			});

			_icon = new Image();
			_icon.style.marginTop = StyleKeyword.Auto;
			_icon.style.marginBottom = StyleKeyword.Auto;
			_icon.style.width = 12;
			_icon.style.height = 12;
			row.AddChildren(_icon, new Spacer(width:8f), header);
			base.Add(row);
			_content = new Column();
			base.Add(_content);
			Expanded = expanded;
		}

		public override Foldout AddChildren(params VisualElement[] children) {
			_content.AddChildren(children);
			return this;
		}

		public new void Add(VisualElement element) {
			_content.Add(element);
		}

		public new void Insert(int index, VisualElement element) {
			_content.Insert(index, element);
		}
	}
}