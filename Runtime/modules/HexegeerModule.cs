using System.Collections.Generic;
using System.Threading.Tasks;
using hexegeer.internallib;

namespace hexegeer {
	public abstract class HexegeerModule<T1> : HexegeerModule 
		where T1 : HexegeerModule<T1>, new() {
		protected static T1 _instance;
		private static Task _launchProcess;

		static HexegeerModule() {
			_instance = new T1();
			_launchProcess = null;
		}
		protected HexegeerModule() {
			AssignModule(this);
		}

		public static async Task Launch() {
			if (_launchProcess == null) {
				try {
					D.Log($"Start Launch: {typeof(T1).Name}");
					_launchProcess = _instance.LaunchInternal();
					await _launchProcess;
					_launchProcess = null;
					D.Log($"Complete Launch: {typeof(T1).Name}");
				} catch (RequireException e) {
					D.LogE($"Require module not active: {e.Type.Name}");
				}
			}
		}

		public static async Task Discard() {
			await _instance.DiscardInternal();
		}

		protected abstract Task LaunchInternal();
		protected abstract Task DiscardInternal();
	}

	public abstract class HexegeerModule<T1, T2> : HexegeerModule<T1>
		where T1 : HexegeerModule<T1, T2>, new()
		where T2 : HexegeerModuleInternal, new() {

		private static T2 _internalInstance;
		protected static T2 Internal => _internalInstance;

		protected override bool Active => _internalInstance.IsActive;
		public static bool IsActive => _internalInstance.IsActive;

		static HexegeerModule() {
			_internalInstance = new T2();
		}
		
		protected sealed override async Task LaunchInternal() {
			if (!_internalInstance.IsActive) {
				await LaunchModuleProcess();
				_internalInstance.IsActive = true;
			}
		}

		protected sealed override async Task DiscardInternal() {
			if (_internalInstance.IsActive) {
				_internalInstance.IsActive = false;
				await _instance.DiscardModuleProcess();
			}
		}

		protected abstract Task LaunchModuleProcess();
		protected abstract Task DiscardModuleProcess();
	}

	public abstract class HexegeerModule {
		private static Dictionary<System.Type, HexegeerModule> _modules;
		protected abstract bool Active { get; }

		static HexegeerModule() {
			_modules = new Dictionary<System.Type, HexegeerModule>();
		}

		protected void AssignModule(HexegeerModule module) {
			_modules.Add(module.GetType(), module);
		}

		private bool ModuleActive<T>() {
			return _modules.TryGetValue(typeof(T), out HexegeerModule module) && module.Active;
		}

		/// <summary>
		/// LaunchModuleProcess()で使用する。
		/// </summary>
		protected void Require<T>() where T : HexegeerModule {
			if (!ModuleActive<T>()) { throw new RequireException(typeof(T)); }
		}
	}

	internal class RequireException : System.Exception { 
		private System.Type _type;
		public System.Type Type => _type;

		public RequireException(System.Type type) {
			_type = type;
		}
	}

}
