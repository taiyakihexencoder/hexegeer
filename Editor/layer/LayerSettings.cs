using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Layer/LayerSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class LayerSettings : ScriptableSingleton<LayerSettings> {
		public const int LAYER_COUNT = 32;

		[SerializeField]
		private int[] _layerIndices = new int[0];

		public int[] LayerIndices {
			get => _layerIndices;
			set {
				_layerIndices = value;
				Save(true);
			}
		}

		[SerializeField]
		private string[] _layerNames = new string[0];
		public string[] LayerNames {
			get => _layerNames;
			set {
				_layerNames = value;
				Save(true);
			}
		}

		[SerializeField]
		private bool[] _table = new bool[0];

		private void OnEnable() {
			if (_layerNames.Length != LAYER_COUNT) {
				_layerIndices = new int[LAYER_COUNT];
				for (int i = 0; i < _layerIndices.Length; ++i) {
					_layerIndices[i] = i;
				}

				_layerNames = new string[LAYER_COUNT];
				DefaultLayer[] layers = System.Enum.GetValues(typeof(DefaultLayer)) as DefaultLayer[];
				for (int i = 0; i < layers.Length; ++i) {
					_layerNames[i] = layers[i].ToString();
				}

				_table = new bool[LAYER_COUNT * LAYER_COUNT];
				for (int i = 0; i < layers.Length; ++i) {
					DefaultLayer[] hitLayers = DefaultLayerCollision.Value[i];
					for (int j = 0; j < hitLayers.Length; ++j) {
						int hitLayer = (int)hitLayers[j];
						_table[i * LAYER_COUNT + j] = true;
						_table[i + j * LAYER_COUNT] = true;
					}
				}
			}
		}

		public void LayerIndex(int index, int value) {
			_layerIndices[index] = value;
			Save(true);
		}

		public int LayerIndex(int index) {
			return _layerIndices[index];
		}

		public void LayerName(int index, string name) {
			_layerNames[index] = name;
			Save(true);
		}
		public string LayerName(int index) {
			return _layerNames[index];
		}

		public bool Table(int a, int b) {
			return _table[a * LAYER_COUNT + b];
		}

		public void Table(int a, int b, bool value) {
			_table[a * LAYER_COUNT + b] = value;
			_table[a + b * LAYER_COUNT] = value;
			Save(true);
		}
	}
}