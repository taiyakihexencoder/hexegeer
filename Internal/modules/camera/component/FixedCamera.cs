using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct FixedCamera : IComponentData, IEnableableComponent {
		public float3 position;
		public quaternion rotation;
	}
}