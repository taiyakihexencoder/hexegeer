using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct DamageObjectSpawnRequest : IComponentData {
		public int id;
		public float3 position;
		public quaternion rotation;
		public Entity owner;
	}
}