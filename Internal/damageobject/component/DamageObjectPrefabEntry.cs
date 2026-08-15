using Unity.Entities;

namespace hexegeer.internallib {
	public struct DamageObjectPrefabEntry : IComponentData {
		public int id;
		public Entity prefab;
	}
}