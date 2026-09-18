using System.Threading.Tasks;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerRuntimeModuleSystemGroup))]
	public partial class HexegeerInputModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerInputModuleTag>();
		}
	}
	
	public struct HexegeerInputModuleTag : IComponentData { }

	public class HexegeerInputModuleInternal : HexegeerModuleInternal { 
		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerInputModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerInputModuleTag>();
		}
	}
}
