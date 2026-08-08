using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace hexegeer {
	using System.IO;
	using internallib;

	public static class HexegeerRuntimeManager {
		/// <summary>
		/// ワールド読み込み完了確認用クエリ
		/// </summary>
		private static EntityQuery _worldIsReadyQuery;

		/// <summary>
		/// ワールド存在確認用クエリ
		/// </summary>
		private static EntityQuery _worldInstanceQuery;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Init() {
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			_worldIsReadyQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<HexegeerWorldReady>()
				.Build(entityManager);

			_worldInstanceQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<HexegeerWorldInstance>()
				.Build(entityManager);
		}

		public static void Boot() {
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			HexegeerManager.BootSystem(entityManager);
		}

		public static async Task StartWorld() {
			SyncContext.Post(() => {
				EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				HexegeerManager.StartWorld(entityManager);
			});

			await HexegeerUtility.ECS.WaitQueryExists(_worldIsReadyQuery);
		}

		public static async Task EndWorld() {
			SyncContext.Post(() => {
				EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				HexegeerManager.EndWorld(entityManager);
			});

			await HexegeerUtility.ECS.WaitQueryEmpty(_worldInstanceQuery);
		}

		public static void Shutdown() {
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			HexegeerManager.ShutdownSystem(entityManager);
		}

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