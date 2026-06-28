using UnityEngine;
using System.Threading;

namespace hexegeer.internallib {
	/// <summary>
	/// Thread内でメインスレッド処理を行う。
	/// </summary>
	public class SyncContext {
		private static SyncContext context;

		private SynchronizationContext sync;

		private SyncContext() {
			sync = SynchronizationContext.Current;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		internal static void CreateContext() {
			context = new SyncContext();
		}

		public static void Send(System.Action action) => context.SendInternal(action);
		public static T Send<T>(System.Func<T> action) => context.SendInternal(action);
		public static void Post(System.Action action) => context.PostInternal(action);

		private void SendInternal(System.Action action) {
			sync.Send(_ => action(), null);
		}

		private T SendInternal<T>(System.Func<T> action) {
			T value = default;
			sync.Send(_ => value = action(), null);
			return value;
		}

		private void PostInternal(System.Action action) {
			sync.Post(_ => action(), null);
		}
	}
}