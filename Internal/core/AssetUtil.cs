using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace hexegeer.internallib {
	public class AssetUtil {
		private static AssetUtil _instance = null;
		private static AssetUtil Instance {
			get {
				if (_instance == null) {
					_instance = new AssetUtil();
				}
				return _instance;
			}
		}

		private Dictionary<string, IResourceHolder> _resourceList;

		private AssetUtil() {
			_resourceList = new Dictionary<string, IResourceHolder>();
		}

		public static async Task<T> RequestLoad<T>(string address) where T : Object {
			IResourceHolder holder = Instance.RequestLoadInternal<T>(address);
			T original = default;
			if (holder.TryGetResource(out object obj)) {
				original = obj as T;
			} else {
				original = (await holder.LoadTask) as T;
			}

			if (original == null) {
				D.LogE($"Resource cannot load: {address}(type = {typeof(T).Name})");
			}

			holder.IncrementReferenceCount();
			return Object.Instantiate(original);
		}

		public static void Release(string address) {
			if (Instance._resourceList.TryGetValue(address, out IResourceHolder holder)) {
				if (holder.DecrementReferenceCount()) {
					holder.Dispose();
					Instance._resourceList.Remove(address);
				}
			}
		}

		public static async Task<List<T>> RequestLoadSubAssets<T>(string mainAddress, string[] subassets) where T : Object {
			IResourceHolder holder = Instance.RequestLoadListInternal<T>(mainAddress, subassets);
			List<T> objList = null;
			if (holder.TryGetResource(out object obj)) {
				objList = obj as List<T>;
			} else {
				objList = (await holder.LoadTask) as List<T>;
			}

			if (objList == null) {
				D.LogE($"Resource cannot load: {mainAddress}(type = List<{typeof(T).Name}>)");
			}
			holder.IncrementReferenceCount();

			List<T> instances = new List<T>();
			foreach(T original in objList) {
				instances.Add(Object.Instantiate(original));
			}
			return instances;
		}

		/// <summary>
		/// 読み込み後に即破棄する。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="address"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static async Task LoadTemp<T>(string address, System.Action<T> callback) where T : Object {
			AsyncOperationHandle<T> op = SyncContext.Send(() => Addressables.LoadAssetAsync<T>(address));
			callback(await op.Task);
			SyncContext.Post(() => Addressables.Release(op));
		}

		private IResourceHolder RequestLoadListInternal<T>(string address, string[] subasset) where T : Object {
			if (!_resourceList.TryGetValue(address, out IResourceHolder holder)) {
				List<AsyncOperationHandle<T>> ops = new List<AsyncOperationHandle<T>>();
				for(int i = 0; i < subasset.Length; ++i) {
					AsyncOperationHandle<T> op = SyncContext.Send(() => Addressables.LoadAssetAsync<T>($"{address}[{subasset[i]}]"));
					ops.Add(op);
				}
				holder = new ListResourceHolder<T>(ops);
				_resourceList.Add(address, holder);
			}
			return holder;
		}

		private IResourceHolder RequestLoadInternal<T>(string address) where T : Object {
			if (!_resourceList.TryGetValue(address, out IResourceHolder holder)) {
				AsyncOperationHandle<T> op = SyncContext.Send(() => Addressables.LoadAssetAsync<T>(address));
				holder = new ResourceHolder<T>(op);
				_resourceList.Add(address, holder);
			}
			return holder;
		}
	}

	internal interface IResourceHolder {
		int ReferenceCount { get; }
		Task<object> LoadTask { get; }
		bool TryGetResource(out object obj);

		void IncrementReferenceCount();
		bool DecrementReferenceCount();

		void Dispose();
	}

	internal sealed class ListResourceHolder<T> : IResourceHolder where T : Object {
		private IList<AsyncOperationHandle<T>> _ops;
		private int _referenceCount;
		private List<T> _resource;

		internal ListResourceHolder(IList<AsyncOperationHandle<T>> ops) {
			_ops = ops;
			_referenceCount = 0;
			_resource = null;
		}

		int IResourceHolder.ReferenceCount => _referenceCount;
		bool IResourceHolder.TryGetResource(out object obj){
			obj = _resource;
			return _resource != null;
		}

		Task<object> IResourceHolder.LoadTask {
			get {
				return Task.Run(async () => {
					List<T> objectList = new List<T>();
					foreach(AsyncOperationHandle<T> op in _ops) { objectList.Add(await op.Task); }
					return objectList as object;
				});
			}
		}

		void IResourceHolder.IncrementReferenceCount(){ _referenceCount++; }

		bool IResourceHolder.DecrementReferenceCount() { 
			_referenceCount--; 
			return _referenceCount <= 0;
		}

		void IResourceHolder.Dispose() {
			foreach(AsyncOperationHandle<T> op in _ops) {
				Addressables.Release(op);
			}
		}
	}

	internal sealed class ResourceHolder<T> : IResourceHolder where T : Object {
		private AsyncOperationHandle<T> _op;
		private int _referenceCount;
		private T _resource;

		internal ResourceHolder(AsyncOperationHandle<T> op) {
			_op = op;
			_referenceCount = 0;
			_resource = null;
		}

		int IResourceHolder.ReferenceCount => _referenceCount;

		bool IResourceHolder.TryGetResource(out object obj) {
			obj = _resource;
			return _resource != null;
		}

		Task<object> IResourceHolder.LoadTask {
			get {
				return Task.Run(async () => {
					_resource = await _op.Task;
					return _resource as object;
				});
			}
		}

		void IResourceHolder.IncrementReferenceCount(){ _referenceCount++; }

		bool IResourceHolder.DecrementReferenceCount() { 
			_referenceCount--; 
			return _referenceCount <= 0;
		}

		void IResourceHolder.Dispose() { Addressables.Release(_op); }
	}

}