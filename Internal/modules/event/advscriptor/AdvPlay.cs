using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	public struct AdvPlay : IComponentData {
		public FixedString64Bytes address;
	}
}