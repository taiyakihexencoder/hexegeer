using System.Threading.Tasks;
using UnityEngine;

namespace hexegeer {
	using System.IO;
	using internallib;

	public static class HexegeerRuntimeManager {
		public static void SetFrameRate(int frameRate) {
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = frameRate;
		}

		// -- Save -- //
		public static UserSaveParameter GetDefaultSaveData() {
			return UserSaveParameter.defaultValue;
		}

		public static bool ExistsUserData(string path) {
			return PersistentData.Exists($"save{Path.DirectorySeparatorChar}{path}");
		}

		/// <summary>
		/// セーブデータの読込
		/// </summary>
		public async static Task LoadUserData(IUserSaveAccessor accessor, string path, System.Action<UserSaveParameter> callback, System.Action<System.Exception> onError) {
			await PersistentData.Load(
				$"save{Path.DirectorySeparatorChar}{path}",
				accessor.deserializer, 
				(data,e) => {
					if (e != null) {
						onError(e);
					} else {
						callback(data);
					}
				}
			);
		}

		/// <summary>
		/// セーブデータの書込
		/// </summary>
		public async static Task SaveUserData(IUserSaveAccessor accessor, UserSaveParameter data, string path, System.Action callback, System.Action<System.Exception> onError) {
			await PersistentData.Save(
				$"save{Path.DirectorySeparatorChar}{path}",
				data,
				accessor.serializer,
				(e) => {
					if (e != null) {
						onError(e);
					} else {
						callback();
					}
				}
			);
		}
	}
}