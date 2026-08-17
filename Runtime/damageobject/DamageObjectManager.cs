using hexegeer.internallib;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public static class DamageObjectManager {
		public static void Request(
			EntityCommandBuffer commandBuffer, 
			DamageObjectId id,
			Entity owner,
			float3 position,
			quaternion rotation
		) {
			Entity entity = commandBuffer.CreateEntity();
			commandBuffer.AddComponent(
				entity, 
				new DamageObjectSpawnRequest {
					id = id.Id,
					owner = owner
				}
			);
			DynamicBuffer<DamageObjectTrailDefinition> trails = commandBuffer.AddBuffer<DamageObjectTrailDefinition>(entity);
			trails.Add(
				new DamageObjectTrailDefinition{
					position = position,
					rotation = rotation,
					limitedLifeTime = 100f,
				}
			);
		}
	}
}