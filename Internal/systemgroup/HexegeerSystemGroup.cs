using Unity.Entities;
using Unity.Physics.Systems;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial class HexegeerSimulationSystemGroup : ComponentSystemGroup {	}

	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerInternalSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
	public partial class HexegeerAfterPhysicsSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerAfterPhysicsSystemGroup))]
	public partial class HexegeerColliderGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerCharacterSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerContentKeySystemGroup : ComponentSystemGroup { }


	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerFieldSystemGroup : ComponentSystemGroup {
	}

	[UpdateInGroup(typeof(HexegeerFieldSystemGroup))]
	public partial class HexegeerFieldInternalSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<FieldSetting>();
		}
	}


	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerInputSystemGroup : ComponentSystemGroup { }
}