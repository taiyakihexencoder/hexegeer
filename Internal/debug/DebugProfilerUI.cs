using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	public sealed class DebugProfilerUI : MonoBehaviour {
		[SerializeField]
		private PanelRenderer _doc;

		private Label _fpsLabel = null;

		public static DebugProfilerUI Load() {
			DebugProfilerUI debugProfilerUI = Resources.Load<DebugProfilerUI>("DebugProfilerUI");
			GameObject instance = Instantiate(debugProfilerUI.gameObject);
			instance.name = "Debug Profiler UI";
			DontDestroyOnLoad(instance);
			return instance.GetComponent<DebugProfilerUI>();
		}

		private void OnEnable() {
			_doc.RegisterUIReloadCallback(OnUiLoad);
		}

		private void OnDisable() {
			_doc.UnregisterUIReloadCallback(OnUiLoad);
		}

		private void OnUiLoad(PanelRenderer renderer, VisualElement root) {
			_fpsLabel = root.Q<Label>("debug-fps-value");
		}

		public void SetFPS(double fps) {
			if (_fpsLabel != null) {
				_fpsLabel.text = $"{fps.ToString("0.00")}";
			}
		}
	}
}