using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerFieldModuleSystemGroup))]
	public partial struct FieldUpdateUnloadExcludeSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAllRW<FieldUnloadExclude>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			state.Dependency = new Job {
				aliveTime = 180.0f,
				dt = SystemAPI.Time.DeltaTime,
				commandBuffer = CreateCommandBuffer(ref state),
			}.Schedule(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct Job : IJobEntity {
			[ReadOnly] public float aliveTime;
			[ReadOnly] public float dt;

			public EntityCommandBuffer commandBuffer;

			void Execute(in Entity entity, RefRW<FieldUnloadExclude> component) {
				if (component.ValueRO.elapsed >= aliveTime) {
					commandBuffer.RemoveComponent<FieldUnloadExclude>(entity);
				} else {
					component.ValueRW.elapsed += dt;
				}
			}
		}
	}
}
