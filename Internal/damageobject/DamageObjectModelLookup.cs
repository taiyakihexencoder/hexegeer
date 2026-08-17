using System.Collections.Generic;

namespace hexegeer.internallib {
	public static class DamageObjectModelLookup {
		private static readonly object _lockHandle = new object();
		private static Dictionary<int, string> _table;

		static DamageObjectModelLookup() {
			_table = new Dictionary<int, string>();
		}

		public static void Register(in DamageObjectTable.DamageObject damageObject) {
			lock (_lockHandle) {
				if (!_table.ContainsKey(damageObject.id)) {
					_table.Add(damageObject.id, damageObject.asset);
				}
			}
		}

		public static bool TryGetAssetAddress(int id, out string address) {
			return _table.TryGetValue(id, out address);
		}
	}
}