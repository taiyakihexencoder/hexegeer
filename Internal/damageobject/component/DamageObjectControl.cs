using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct DamageObjectControl : IComponentData {
		public int damageObjectId;
		public int entityIndex;
		public float3 startPosition;
		public quaternion startRotation;
		public DamageObjectExtra extra;
	}
}