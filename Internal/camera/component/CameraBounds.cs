using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct CameraBounds : IComponentData, IEnableableComponent {
		public float3 boundsMin;
		public float3 boundsMax;
	}
}