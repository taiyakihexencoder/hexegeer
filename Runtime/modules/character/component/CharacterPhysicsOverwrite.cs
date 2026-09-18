using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public struct CharacterPhysicsOverwrite : IComponentData, IEnableableComponent {
		public bool overwriteVelocity;
		public float3 velocity;

		public bool overwriteSnapToGround;
		public bool snapToGround;

		public bool overwriteIgnoreSnapToGround;
		public bool ignoreSnapToGround;
	}
}