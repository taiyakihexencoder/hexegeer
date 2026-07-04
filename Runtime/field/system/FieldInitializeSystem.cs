using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerFieldSystemGroup))]
	public partial class FieldInitializeSystem : SystemBase {
		private EntityQuery _launchQuery;

		protected override void OnCreate() {
			_launchQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<LaunchFieldSystemRequest>()
				.Build(EntityManager);
			RequireForUpdate(_launchQuery);
		}

		protected override void OnUpdate() {
			EntityManager.DestroyEntity(_launchQuery);

			FieldSettingGenerator.Generate(EntityManager);
		}
	}
}