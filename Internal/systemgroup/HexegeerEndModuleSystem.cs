using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial struct HexegeerEndModuleSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<HexegeerEndSystemModuleRequest>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			bool eventChecked = false;
			EntityCommandBuffer commandBuffer = CreateCommandBuffer(ref state);

			NativeArray<HexegeerEndSystemModuleRequest> requests = _query.ToComponentDataArray<HexegeerEndSystemModuleRequest>(Allocator.Temp);
			foreach(HexegeerEndSystemModuleRequest request in requests) {
				switch (request.module) {
					case HexegeerSystemModule.Event: {
						if (!eventChecked && SystemAPI.TryGetSingletonEntity<HexegeerSystemModuleEventComponent>(out Entity singleton)) {
							commandBuffer.DestroyEntity(singleton);
							eventChecked = true;
						}
						break;
					}
				}
			}

			commandBuffer.DestroyEntity(_query, EntityQueryCaptureMode.AtPlayback);
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
