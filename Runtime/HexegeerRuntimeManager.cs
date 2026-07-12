using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace hexegeer {
	using internallib;

	public static class HexegeerRuntimeManager {
		/// <summary>
		/// フィールド用の設定データ
		/// </summary>
		private static EntityQuery _fieldSettingQuery;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Init() {
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			_fieldSettingQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<FieldSetting>()
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

			await HexegeerUtility.ECS.WaitQueryExists(_fieldSettingQuery);
		}

		public static async Task EndWorld() {
			SyncContext.Post(() => {
				EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				HexegeerManager.EndWorld(entityManager);
			});

			await HexegeerUtility.ECS.WaitQueryEmpty(_fieldSettingQuery);
		}

		public static void Shutdown() {
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			HexegeerManager.ShutdownSystem(entityManager);
		}
	}
}