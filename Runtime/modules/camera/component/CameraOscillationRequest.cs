using hexegeer.internallib;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public struct CameraOscillationRequest : IComponentData {
		public CameraOscillationType type;
		public float3 direction;
		public float level;
		public float speed;
		public float seconds;

		public static CameraOscillationRequest Once(
			float3 direction,
			float level,
			float seconds
		) {
			return new CameraOscillationRequest {
				type = CameraOscillationType.Once,
				direction = direction,
				level = level,
				speed = 1f,
				seconds = seconds,
			};
		}
	}
}