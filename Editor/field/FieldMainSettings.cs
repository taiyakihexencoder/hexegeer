using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Field/FieldMainSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	internal sealed class FieldMainSettings : ScriptableSingleton<FieldMainSettings> {
		[SerializeField]
		private FieldViewType _viewType;
		internal FieldViewType ViewType {
			get => _viewType;
			set {
				if (_viewType != value) {
					_viewType = value;
					Save(true);
				}
			}
		}

		[SerializeField]
		private float _loadFieldDistance = 20.0f;
		internal float LoadFieldDistance {
			get => _loadFieldDistance;
			set {
				if (_loadFieldDistance != value) {
					_loadFieldDistance = value;
					Save(true);
				}
			}
		}

		[SerializeField]
		private float _unloadfieldDistance = 50.0f;
		internal float UnloadFieldDistance {
			get => _unloadfieldDistance;
			set {
				if (_unloadfieldDistance != value) {
					_unloadfieldDistance = value;
					Save(true);
				}
			}
		}

		[SerializeField]
		private int _meshCacheCount = 8;
		internal int MeshCacheCount {
			get => _meshCacheCount;
			set {
				if (_meshCacheCount != value) {
					_meshCacheCount = value;
					Save(true);
				}
			}
		}

		[SerializeField]
		private double _updateInterval = 1.0f;
		internal double UpdateInterval {
			get => _updateInterval;
			set {
				if(_updateInterval != value) {
					_updateInterval = value;
					Save(true);
				}
			}
		}
	}
}