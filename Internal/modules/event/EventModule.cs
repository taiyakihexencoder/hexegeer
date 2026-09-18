using System.Threading.Tasks;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerWorldModuleSystemGroup))]
	public partial class HexegeerEventModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerEventModuleTag>();
		}
	}

	[UpdateInGroup(typeof(HexegeerWorldModuleSystemGroup))]
	public partial class HexegeerEventModuleRunningSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerEventModuleRunningTag>();
		}
	}
	
	[UpdateInGroup(typeof(HexegeerWorldModuleAfterColliderSystemGroup))]
	public partial class HexegeerEventModuleAfterColliderSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerEventModuleRunningTag>();
		}
	}

	public struct HexegeerEventModuleTag : IComponentData { }
	public struct HexegeerEventModuleRunningTag : IComponentData { }

	public class HexegeerEventModuleInternal : HexegeerModuleInternal { 
		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerEventModuleTag());
		}

		public async Task CreateModuleRunningTag() {
			await CreateInstance(new HexegeerEventModuleRunningTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerEventModuleTag>();
		}

		public async Task DeleteModuleRunningTag() {
			await DeleteInstance<HexegeerEventModuleRunningTag>();
		}
	}
}
