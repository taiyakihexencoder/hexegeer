using Unity.Entities;
using Unity.Physics.Systems;

namespace hexegeer.internallib {
	/// <summary>
	/// Boot関係の初期化なしで動かすSystem
	/// </summary>
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial class HexegeerGlobalSystemGroup : ComponentSystemGroup { }

	/// <summary>
	/// Boot, 物理、Transform以外のSystemのルート
	/// </summary>
	[UpdateInGroup(typeof(HexegeerGlobalSystemGroup))]
	public partial class HexegeerSimulationSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerSystemInstance>();
		}
	}

	[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
	public partial class HexegeerBeforePhysicsSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerWorldInstance>();
		}
	}


	[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
	public partial class HexegeerAfterPhysicsSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerAfterPhysicsSystemGroup))]
	public partial class HexegeerColliderGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerAfterPhysicsSystemGroup))]
	public partial class HexegeerCameraSystemGroup : ComponentSystemGroup {
		protected override void OnCreate() {
			base.OnCreate();
			RequireForUpdate<CameraInstance>();
		}
	}


	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerInternalSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerWorldSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerWorldInstance>();
		}
	}



	[UpdateInGroup(typeof(HexegeerWorldSystemGroup))]
	public partial class HexegeerCharacterSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerWorldSystemGroup))]
	public partial class HexegeerDamageObjectSystemGroup : ComponentSystemGroup { }

	[UpdateInGroup(typeof(HexegeerWorldSystemGroup))]
	public partial class HexegeerContentKeySystemGroup : ComponentSystemGroup { }


	[UpdateInGroup(typeof(HexegeerWorldSystemGroup))]
	public partial class HexegeerFieldSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<FieldSetting>();
		}
	}


	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class HexegeerInputSystemGroup : ComponentSystemGroup { }
}