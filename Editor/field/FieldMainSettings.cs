using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class FieldMainSettings : ScriptableSingleton<FieldMainSettings> {
		[SerializeField]
		private FieldViewType _viewType;
		public FieldViewType ViewType {
			get => _viewType;
			set {
				if (_viewType != value) {
					_viewType = value;
					Save(true);
				}
			}
		}
	}
}