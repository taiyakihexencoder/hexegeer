using System.Threading.Tasks;
using Unity.Entities;
using Unity.Physics.Systems;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial class HexegeerRuntimeModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerRuntimeModuleTag>();
		}
	}

	[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
	public partial class HexegeerRuntimeModuleAfterPhysicsSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerRuntimeModuleTag>();
		}
	}


	[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
	public partial class HexegeerRuntimeModuleColliderSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerRuntimeModuleTag>();
		}
	}

	[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
	[UpdateAfter(typeof(HexegeerRuntimeModuleColliderSystemGroup))]
	public partial class HexegeerRuntimeModuleAfterColliderSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerRuntimeModuleTag>();
		}
	}
	
	public struct HexegeerRuntimeModuleTag : IComponentData { }

	public class HexegeerRuntimeModuleInternal : HexegeerModuleInternal { 
		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerRuntimeModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerRuntimeModuleTag>();
		}
	}
}
