using Unity.Entities;

namespace hexegeer {
	public struct InputSideButton : IComponentData {
		public bool bumperL;
		public bool bumperR;
		public bool triggerL;
		public bool triggerR;
		public bool stickL;
		public bool stickR;
	}
}