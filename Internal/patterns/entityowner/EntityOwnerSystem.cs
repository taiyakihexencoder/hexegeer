using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial struct EntityOwnerSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<EntityOwner>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			EntityCommandBuffer commandBuffer = CreateCommandBuffer(ref state);
			NativeArray<Entity> array = _query.ToEntityArray(Allocator.Temp);
			Entity owner = Entity.Null;
			for(int i = 0; i < array.Length; ++i) {
				owner = state.EntityManager.GetComponentData<EntityOwner>(array[i]).owner;
				if (owner == Entity.Null || !state.EntityManager.Exists(owner)) {
					commandBuffer.DestroyEntity(array[i]);
				}
			}
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}
	}
}
