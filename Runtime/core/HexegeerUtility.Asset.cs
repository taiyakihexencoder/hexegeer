using System.Threading.Tasks;

namespace hexegeer {
	using System.Collections.Generic;
	using internallib;
	using UnityEngine;

	public static partial class HexegeerUtility {
		public static class Asset {
			public static async Task<T> RequestLoad<T>(string address) where T : Object {
				return await AssetUtil.RequestLoad<T>(address);
			}

			public static async Task<List<T>> RequestLoadSubAssets<T>(string address, string[] subassets) where T : Object {
				return await AssetUtil.RequestLoadSubAssets<T>(address, subassets);
			}

			public static void Release(string address) {
				AssetUtil.Release(address);
			}

			public static async Task LoadTemp<T>(string address, System.Action<T> callback) where T : Object {
				await AssetUtil.LoadTemp(address, callback);
			}
		}
	}
}