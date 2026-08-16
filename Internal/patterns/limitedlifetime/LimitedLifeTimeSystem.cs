using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial struct LimitedLifeTimeSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAllRW<LimitedLifeTime>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			state.Dependency = new ProcessJob {
				commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter(),
				deltaTime = SystemAPI.Time.DeltaTime,
			}.ScheduleParallel(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct ProcessJob : IJobEntity {
			public EntityCommandBuffer.ParallelWriter commandBuffer;
			[ReadOnly] public float deltaTime;

			void Execute([EntityIndexInQuery] int sortKey, in Entity entity, RefRW<LimitedLifeTime> lifeTime) {
				if (lifeTime.ValueRO.seconds < deltaTime) {
					commandBuffer.DestroyEntity(sortKey, entity);
				} else {
					lifeTime.ValueRW.seconds -= deltaTime;
				}
			}
		}
	}
}
