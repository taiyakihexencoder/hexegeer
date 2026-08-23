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
						CameraBounds cameraBounds = SystemAPI.GetComponent<CameraBounds>(cameraEntity);
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

				RefRW<CameraOscillation> oscillation = SystemAPI.GetComponentRW<CameraOscillation>(cameraEntity);
				if (oscillation.ValueRO.type != CameraOscillationType.None) {
					float alpha = oscillation.ValueRO.elapsed / oscillation.ValueRO.seconds;
					switch (oscillation.ValueRO.type) {
						case CameraOscillationType.Once: {
							localTransform.ValueRW.Position += oscillation.ValueRO.level * math.exp(- alpha * 10f) * oscillation.ValueRO.direction;
							break;
						}

						case CameraOscillationType.Sine: {
							localTransform.ValueRW.Position += oscillation.ValueRO.level * math.sin(alpha * math.PI2 * oscillation.ValueRO.speed) * oscillation.ValueRO.direction;
							break;
						}
					}

					oscillation.ValueRW.elapsed += SystemAPI.Time.DeltaTime;
					if (oscillation.ValueRO.elapsed > oscillation.ValueRO.seconds) {
						oscillation.ValueRW.type = CameraOscillationType.None;
						oscillation.ValueRW.direction = float3.zero;
						oscillation.ValueRW.speed = 0f;
						oscillation.ValueRW.seconds = 1f;
						oscillation.ValueRW.level = 0f;
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
