using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerCameraSystemGroup)), UpdateAfter(typeof(CameraSystem))]
	public partial class CameraUpdateSystem : SystemBase {
		private EntityQuery _query;

		protected override void OnCreate() {
			base.OnCreate();
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CameraInstance, LocalToWorld>()
				.Build(EntityManager);
			RequireForUpdate(_query);
		}

		protected override void OnUpdate() {
			LocalToWorld localToWorld = _query.GetSingleton<LocalToWorld>();
			Transform cameraTransform = Camera.main.transform;
			cameraTransform.SetPositionAndRotation(
				Vector3.Lerp(cameraTransform.position, localToWorld.Position, 0.2f),
				Quaternion.Slerp(cameraTransform.rotation, localToWorld.Rotation, 0.2f)
			);
		}
	}
}