using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerContentKeySystemGroup))]
	public partial struct ContentKeyUnloadSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<ContentKeyUnloadRequest>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			EntityCommandBuffer commandBuffer = CreateCommandBuffer(ref state);

			state.Dependency = new UnloadJob {
				commandBuffer = commandBuffer,
			}.Schedule(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct UnloadJob : IJobEntity {
			public EntityCommandBuffer commandBuffer;

			void Execute(in Entity entity, RefRO<ContentKeyUnloadRequest> request) {
				Entity eventPointDeleteEntity = commandBuffer.CreateEntity();
				commandBuffer.AddComponent(eventPointDeleteEntity, new EventPointDeleteRequest { contentKey = request.ValueRO.contentKey, });

				commandBuffer.DestroyEntity(entity);
			}
		}
	}
}
