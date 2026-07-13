using hexegeer.internallib;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public static class CharacterManager {
		public static void RequestSpawn(
			EntityManager entityManager,
			CharacterId characterId, 
			float3 position, 
			quaternion rotation
		) {
			entityManager.Create(
				new CharacterSpawnRequest {
					id = characterId.Id,
					position = position,
					rotation = rotation,
				}
			);
		}
	}
}