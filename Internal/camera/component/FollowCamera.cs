using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct FollowCamera : IComponentData, IEnableableComponent {
		public Entity target;
		public quaternion direction;
		public float distance;
		public float3 offset;
	}
}