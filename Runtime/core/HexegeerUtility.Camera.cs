using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer {
	public static partial class HexegeerUtility {
		public static class Camera {
			private static EntityQuery _query;

			[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
			private static void Init() {
				_query = new EntityQueryBuilder(Allocator.Temp)
					.WithAll<CameraInstance>()
					.Build(World.DefaultGameObjectInjectionWorld.EntityManager);
			}

			public static void StartFixedCamera(EntityManager entityManager, float3 position, quaternion rotation) {
				Entity entity = _query.GetSingletonEntity();

				EnableFixed(entityManager, entity, position, rotation);
				DisableFollow(entityManager, entity);
				DisableLerp(entityManager, entity);
			}

			public static void StartFixedCamera(EntityManager entityManager, float3 position, quaternion rotation, float lerpSeconds) {
				Entity entity = _query.GetSingletonEntity();

				EnableFixed(entityManager, entity, position, rotation);
				DisableFollow(entityManager, entity);
				EnableLerp(entityManager, entity, lerpSeconds);
			}

			public static void StartFollowCamera(EntityManager entityManager, Entity target, float3 offset, quaternion direction, float distance) {
				Entity entity = _query.GetSingletonEntity();

				DisableFixed(entityManager, entity);
				EnableFollow(entityManager, entity, target, offset, direction, distance);
				DisableLerp(entityManager, entity);
			}

			public static void StartFollowCamera(EntityManager entityManager, Entity target, float3 offset, quaternion direction, float distance, float lerpSeconds) {
				Entity entity = _query.GetSingletonEntity();

				DisableFixed(entityManager, entity);
				EnableFollow(entityManager, entity, target, offset, direction, distance);
				EnableLerp(entityManager, entity, lerpSeconds);
			}


			public static void StartFollowCamera(EntityManager entityManager, Entity target, float3 offset, quaternion direction, float distance, float3 boundsMin, float3 boundsMax) {
				Entity entity = _query.GetSingletonEntity();

				DisableFixed(entityManager, entity);
				EnableFollow(entityManager, entity, target, offset, direction, distance, boundsMin, boundsMax);
				DisableLerp(entityManager, entity);
			}

			public static void StartFollowCamera(EntityManager entityManager, Entity target, float3 offset, quaternion direction, float distance, float3 boundsMin, float3 boundsMax, float lerpSeconds) {
				Entity entity = _query.GetSingletonEntity();

				DisableFixed(entityManager, entity);
				EnableFollow(entityManager, entity, target, offset, direction, distance, boundsMin, boundsMax);
				EnableLerp(entityManager, entity, lerpSeconds);
			}

			private static void EnableLerp(EntityManager entityManager, Entity entity, float lerpSeconds) {
				LocalToWorld localToWorld = entityManager.GetComponentData<LocalToWorld>(entity);
				if (!entityManager.IsComponentEnabled<CameraLerp>(entity)) {
					entityManager.SetComponentEnabled<CameraLerp>(entity, true);
				}
				RefRW<CameraLerp> cameraLerp = _query.GetSingletonRW<CameraLerp>();
				cameraLerp.ValueRW.basePosition = localToWorld.Position;
				cameraLerp.ValueRW.baseRotation = localToWorld.Rotation;
				cameraLerp.ValueRW.elapsedSeconds = 0.0f;
				cameraLerp.ValueRW.performSeconds = lerpSeconds;
			}

			private static void DisableLerp(EntityManager entityManager, Entity entity) {
				if (entityManager.IsComponentEnabled<CameraLerp>(entity)) {
					entityManager.SetComponentEnabled<CameraLerp>(entity, false);
				}
			}

			private static void EnableFollow(EntityManager entityManager, Entity entity, Entity target, float3 offset, quaternion direction, float distance) {
				if (!entityManager.IsComponentEnabled<FollowCamera>(entity)) {
					entityManager.SetComponentEnabled<FollowCamera>(entity, true);
				}
				
				if (entityManager.IsComponentEnabled<CameraBounds>(entity)) {
					entityManager.SetComponentEnabled<CameraBounds>(entity, false);
				}

				RefRW<FollowCamera> followCamera = _query.GetSingletonRW<FollowCamera>();
				followCamera.ValueRW.target = target;
				followCamera.ValueRW.offset = offset;
				followCamera.ValueRW.direction = direction;
				followCamera.ValueRW.distance = distance;
			}

			private static void EnableFollow(EntityManager entityManager, Entity entity, Entity target, float3 offset, quaternion direction, float distance, float3 boundsMin, float3 boundsMax) {
				if (!entityManager.IsComponentEnabled<FollowCamera>(entity)) {
					entityManager.SetComponentEnabled<FollowCamera>(entity, true);
				}
				
				if (!entityManager.IsComponentEnabled<CameraBounds>(entity)) {
					entityManager.SetComponentEnabled<CameraBounds>(entity, true);
				}

				RefRW<FollowCamera> followCamera = _query.GetSingletonRW<FollowCamera>();
				followCamera.ValueRW.target = target;
				followCamera.ValueRW.offset = offset;
				followCamera.ValueRW.direction = direction;
				followCamera.ValueRW.distance = distance;

				RefRW<CameraBounds> bounds = _query.GetSingletonRW<CameraBounds>();
				bounds.ValueRW.boundsMin = boundsMin;
				bounds.ValueRW.boundsMax = boundsMax;
			}

			private static void DisableFollow(EntityManager entityManager, Entity entity) {
				if (entityManager.IsComponentEnabled<FollowCamera>(entity)) {
					entityManager.SetComponentEnabled<FollowCamera>(entity, false);

					if (entityManager.IsComponentEnabled<CameraBounds>(entity)) {
						entityManager.SetComponentEnabled<CameraBounds>(entity, false);
					}
				}
			}

			private static void EnableFixed(EntityManager entityManager, Entity entity, float3 position, quaternion rotation) {
				if (!entityManager.IsComponentEnabled<FixedCamera>(entity)) {
					entityManager.SetComponentEnabled<FixedCamera>(entity, true);
				}
				
				RefRW<FixedCamera> fixedCamera = _query.GetSingletonRW<FixedCamera>();
				fixedCamera.ValueRW.position = position;
				fixedCamera.ValueRW.rotation = rotation;
			}

			private static void DisableFixed(EntityManager entityManager, Entity entity) {
				if (entityManager.IsComponentEnabled<FixedCamera>(entity)) {
					entityManager.SetComponentEnabled<FixedCamera>(entity, false);
				}
			}
		}
	}
}