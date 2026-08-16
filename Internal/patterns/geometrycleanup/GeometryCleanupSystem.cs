using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace hexegeer.internallib {
	/// <summary>
	/// PhysicsColliderを持つEntityが破棄されたらgeometryを解放する
	/// </summary>
	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial struct GeometryCleanupSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.WithAll<GeometryCleanup>()
				.WithNone<PhysicsCollider>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			state.Dependency = new ReleaseJob { }.ScheduleParallel(_query, state.Dependency);
		}
	
		/// <summary>
		/// Editorの再生終了など、終了処理の場合はEntityが破棄されずLeakになってしまうので、
		/// OnDestroyですべて破棄する。
		/// </summary>
		void ISystem.OnDestroy(ref SystemState state) {
			EntityQuery destroyQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.WithAll<GeometryCleanup>()
				.Build(ref state);

			NativeArray<GeometryCleanup> array = destroyQuery.ToComponentDataArray<GeometryCleanup>(Allocator.Temp);
			foreach(GeometryCleanup cleanup in array) {
				cleanup.geometry.Dispose();
			}
			array.Dispose();
		}

		partial struct ReleaseJob : IJobEntity {
			void Execute(RefRO<GeometryCleanup> cleanup) {
				cleanup.ValueRO.geometry.Dispose();
			}
		}
	}

}
