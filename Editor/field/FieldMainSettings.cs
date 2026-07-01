using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Field/FieldMainSettings.geer", FilePathAttribute.Location.ProjectFolder)]
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