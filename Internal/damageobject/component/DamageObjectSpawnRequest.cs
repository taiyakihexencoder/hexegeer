using Unity.Entities;

namespace hexegeer.internallib {
	public struct DamageObjectSpawnRequest : IComponentData {
		public int id;
		public Entity owner;
		public DamageObjectExtra extra;
	}
}