namespace hexegeer {
	using internallib;

	public static partial class HexegeerUtility {
		public static class App {
			public static event AppUtil.QuittingHandler quitting {
				add { AppUtil.quitting += value; }
				remove { AppUtil.quitting -= value; }
			}

			public static void Quit() { AppUtil.Quit(); }
		}
	}
}