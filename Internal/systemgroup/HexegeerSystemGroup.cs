using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial class HexegeerSimulationSystemGroup : ComponentSystemGroup {	}

	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerInternalSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerInputSystemGroup : ComponentSystemGroup { }
}