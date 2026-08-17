using Unity.Entities;

namespace hexegeer.internallib {
	public struct DamageObjectCreateModelRequest : IComponentData {
		public Entity observeEntity;
		public int id;
	}
}