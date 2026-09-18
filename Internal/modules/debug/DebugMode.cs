using Unity.Entities;

namespace hexegeer.internallib {
	public struct DebugMode : IComponentData { 

		public static void GenerateInstance() {
			World world = World.DefaultGameObjectInjectionWorld;
			EntityManager entityManager = world.EntityManager;
			Entity entity = entityManager.Create(new DebugMode());
			entityManager.SetName(entity, "Debug Mode");
		}
	}
}