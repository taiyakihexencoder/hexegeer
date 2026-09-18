using Unity.Entities;

namespace hexegeer.internallib {
	public struct LimitedLifeTime : IComponentData {
		public float seconds;
	}
}