using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerCameraSystemGroup))]
	public partial struct CameraSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAllRW<LocalTransform>()
				.WithAll<CameraInstance>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			if (SystemAPI.TryGetSingletonEntity<CameraInstance>(out Entity cameraEntity)) {
				RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(cameraEntity);

				if (SystemAPI.IsComponentEnabled<FixedCamera>(cameraEntity)) {
					FixedCamera fixedCamera = SystemAPI.GetComponent<FixedCamera>(cameraEntity);
					localTransform.ValueRW.Position = fixedCamera.position;
					localTransform.ValueRW.Rotation = fixedCamera.rotation;

				} else if (SystemAPI.IsComponentEnabled<FollowCamera>(cameraEntity)) {
					FollowCamera followCamera = SystemAPI.GetComponent<FollowCamera>(cameraEntity);

					if (followCamera.target != Entity.Null && SystemAPI.HasComponent<LocalToWorld>(followCamera.target)) {
						LocalToWorld localToWorld = SystemAPI.GetComponent<LocalToWorld>(followCamera.target);
						float3 offset = math.mul(localToWorld.Rotation, followCamera.offset);
						localTransform.ValueRW.Position = localToWorld.Position + offset +
							math.mul(followCamera.direction, new float3(0f, 0f, - followCamera.distance));
						localTransform.ValueRW.Rotation = followCamera.direction;
					}

					if (SystemAPI.IsComponentEnabled<CameraBounds>(cameraEntity)) {
						CameraBounds camerBounds = SystemAPI.GetComponent<CameraBounds>(cameraEntity);
					}
				}

				if (SystemAPI.IsComponentEnabled<CameraLerp>(cameraEntity)) {
					RefRW<CameraLerp> cameraLerp = SystemAPI.GetComponentRW<CameraLerp>(cameraEntity);
					cameraLerp.ValueRW.elapsedSeconds += SystemAPI.Time.DeltaTime;
					if (cameraLerp.ValueRO.elapsedSeconds >= cameraLerp.ValueRO.performSeconds) {
						SystemAPI.SetComponentEnabled<CameraLerp>(cameraEntity, false);
					} else {
						float alpha = cameraLerp.ValueRO.elapsedSeconds / cameraLerp.ValueRO.performSeconds;
						localTransform.ValueRW.Position = math.lerp(cameraLerp.ValueRO.basePosition, localTransform.ValueRO.Position, alpha);
						localTransform.ValueRW.Rotation = math.slerp(cameraLerp.ValueRO.baseRotation, localTransform.ValueRO.Rotation, alpha);
					}
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
