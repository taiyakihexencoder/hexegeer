namespace hexegeer {
	public static partial class HexegeerUtility {
		public static class Sync {
			/// <summary>
			/// 非同期処理の途中でメインスレッドから実行
			/// </summary>
			public static void Post(System.Action action) => internallib.SyncContext.Post(action);

			/// <summary>
			/// 非同期処理の途中でメインスレッドから実行
			/// </summary>
			public static void Send(System.Action action) => internallib.SyncContext.Send(action);

			/// <summary>
			/// 非同期処理の途中でメインスレッドから実行
			/// </summary>
			public static T Send<T>(System.Func<T> action) => internallib.SyncContext.Send(action);
		}
	}
}