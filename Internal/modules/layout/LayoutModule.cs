using System.Threading.Tasks;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerWorldModuleSystemGroup))]
	public partial class HexegeerLayoutModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerLayoutModuleTag>();
		}
	}
	
	public struct HexegeerLayoutModuleTag : IComponentData { }

	public class HexegeerLayoutModuleInternal : HexegeerModuleInternal { 
		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerLayoutModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerLayoutModuleTag>();
		}
	}
}
