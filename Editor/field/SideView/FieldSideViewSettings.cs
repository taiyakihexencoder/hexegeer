using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Field/SideViewSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class FieldSideViewSettings : ScriptableSingleton<FieldSideViewSettings> {
		/// <summary>
		/// z軸方向の幅
		/// </summary>
		public float Width {
			get => _width;
			set {
				if (_width != value) {
					_width = value;
					Save(true);
				}
			}
		}
		[SerializeField]
		private float _width = 10.0f;
		
		/// <summary>
		/// z軸オフセット
		/// </summary>
		public float ZOffset {
			get => _zOffset;
			set {
				if (_zOffset != value) {
					_zOffset = value;
					Save(true);
				}
			}
		}
		[SerializeField]
		private float _zOffset = 0.0f;
	}
}