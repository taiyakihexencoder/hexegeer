using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	public partial struct CameraOscillationSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CameraOscillationRequest>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			if (SystemAPI.TryGetSingletonRW(out RefRW<CameraOscillation> oscillation)) {
				EntityCommandBuffer commandBuffer = CreateCommandBuffer(ref state);
				commandBuffer.DestroyEntity(_query, EntityQueryCaptureMode.AtPlayback);

				NativeArray<CameraOscillationRequest> requests = _query.ToComponentDataArray<CameraOscillationRequest>(Allocator.Temp);
				oscillation.ValueRW.elapsed = 0.0f;
				oscillation.ValueRW.type = requests[0].type;
				oscillation.ValueRW.direction = requests[0].direction;
				oscillation.ValueRW.level = requests[0].level;
				oscillation.ValueRW.speed = requests[0].speed;
				oscillation.ValueRW.seconds = requests[0].seconds;
				requests.Dispose();
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
