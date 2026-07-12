using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct CameraLerp : IComponentData, IEnableableComponent {
		public float3 basePosition;
		public quaternion baseRotation;
		public float elapsedSeconds;
		public float performSeconds;
	}
}