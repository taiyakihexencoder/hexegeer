using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerFieldSystemGroup))]
	public partial class FieldTerminateSystem : SystemBase {
		private EntityQuery _terminateQuery;

		protected override void OnCreate() {
			_terminateQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<TerminateFieldSystemRequest>()
				.Build(EntityManager);
			RequireForUpdate(_terminateQuery);
		}

		protected override void OnUpdate() {
			EntityManager.DestroyEntity(_terminateQuery);

			if (SystemAPI.TryGetSingletonEntity<FieldSetting>(out Entity singleton)) {
				EntityManager.DestroyEntity(singleton);
			}
		}
	}
}