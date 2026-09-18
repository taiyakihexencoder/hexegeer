using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace hexegeer.internallib {
	/// <summary>
	/// PhysicsColliderを持つEntityが破棄されたらgeometryを解放する
	/// </summary>
	[UpdateInGroup(typeof(HexegeerRuntimeModuleSystemGroup))]
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
			state.Dependency = new ReleaseJob {
				commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter(),
			}.ScheduleParallel(_query, state.Dependency);
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

			NativeArray<GeometryCleanup> componentArray = destroyQuery.ToComponentDataArray<GeometryCleanup>(Allocator.Temp);
			foreach(GeometryCleanup cleanup in componentArray) {
				cleanup.geometry.Dispose();
			}
			componentArray.Dispose();

			NativeArray<Entity> entityArray = destroyQuery.ToEntityArray(Allocator.Temp);
			foreach(Entity entity in entityArray) {
				state.EntityManager.RemoveComponent<GeometryCleanup>(entity);
			}
			entityArray.Dispose();
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct ReleaseJob : IJobEntity {
			public EntityCommandBuffer.ParallelWriter commandBuffer;

			void Execute([EntityIndexInQuery] int sortKey, in Entity entity, RefRO<GeometryCleanup> cleanup) {
				cleanup.ValueRO.geometry.Dispose();
				commandBuffer.RemoveComponent<GeometryCleanup>(sortKey, entity);
			}
		}
	}

}
