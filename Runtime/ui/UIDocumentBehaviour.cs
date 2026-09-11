using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer {
	public abstract class UIDocumentBehaviour : MonoBehaviour {
		[SerializeField]
		private PanelRenderer _doc = null;

		protected virtual void OnEnable() {
			Enable();
			_doc.RegisterUIReloadCallback(OnUiLoad);
		}

		protected virtual void OnDisable() {
			_doc.UnregisterUIReloadCallback(OnUiLoad);
			Disable();
		}

		protected virtual void Enable() { }
		protected virtual void Disable() { }

		protected abstract void OnUiLoad(PanelRenderer renderer, VisualElement root);
	}

	public abstract class UIDocumentBehaviour<T> : UIDocumentBehaviour where T : UIDocumentBehaviour<T>{
		private static T _instance = null;

		public static T Load() {
			if (_instance == null) {
				T prefab = Resources.Load<T>(typeof(T).Name);
				GameObject instance = Instantiate(prefab.gameObject);
				instance.name = prefab.name;
				DontDestroyOnLoad(instance);
				_instance = instance.GetComponent<T>();
			}
			return _instance;
		}

		public static void Unload() {
			if (_instance != null) {
				Destroy(_instance.gameObject);
				_instance = null;
			}
		}

		protected sealed override void OnEnable() {
			base.OnEnable();
		}

		protected sealed override void OnDisable() {
			base.OnDisable();
		}
	}
}