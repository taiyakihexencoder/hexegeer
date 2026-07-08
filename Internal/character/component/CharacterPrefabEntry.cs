using Unity.Entities;

namespace hexegeer.internallib {
	public struct CharacterPrefabEntry : IComponentData {
		public int id;
		public Entity prefab;
	}
}