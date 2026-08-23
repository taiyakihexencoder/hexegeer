using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct CameraOscillation : IComponentData {
		public CameraOscillationType type;
		public float3 direction;
		public float level;
		public float speed;
		public float elapsed;
		public float seconds;
	}
}