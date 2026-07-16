using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace hexegeer {
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
	}
}