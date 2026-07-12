using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

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
			UnityEngine.Camera.main.transform.SetPositionAndRotation(localToWorld.Position, localToWorld.Rotation);
		}
	}
}