using Unity.Entities;

namespace hexegeer.internallib {
	public struct CharacterCreateModelRequest : IComponentData {
		public Entity observeEntity;
		public int id;
	}
}