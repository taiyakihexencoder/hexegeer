using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct DamageObjectTrailDefinition : IBufferElementData {
		public float3 position;
		public quaternion rotation;
		public float limitedLifeTime;
	}
}