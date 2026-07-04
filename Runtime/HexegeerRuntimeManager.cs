using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace hexegeer {
	using internallib;

	public static class HexegeerRuntimeManager {
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Init() {
			Field = new _Field();
		}

		public class _Field {
			private static EntityQuery _query;

			public _Field() {
				_query = new EntityQueryBuilder(Allocator.Temp)
					.WithAll<FieldSetting>()
					.Build(World.DefaultGameObjectInjectionWorld.EntityManager);
			}

			public void Launch() {
				EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				entityManager.Create(new LaunchFieldSystemRequest{});
			}

			public void Terminate() {
				EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				entityManager.Create(new TerminateFieldSystemRequest{});
			}

			public async Task WaitLaunch() {
				await HexegeerUtility.ECS.WaitQueryExists(_query);
			}

			public async Task WaitTerminate() {
				await HexegeerUtility.ECS.WaitQueryEmpty(_query);
			}
		}

		public static _Field Field { get; private set; }
	}
}