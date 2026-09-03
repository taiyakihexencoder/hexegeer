using hexegeer.internallib;
using Unity.Entities;

namespace hexegeer {
	/// <summary>
	/// エディタ再生時など、急な中断でも解放されるべきリソースを解放する
	/// </summary>
	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial struct HexegeerECSFinalizerSystem : ISystem {

		void ISystem.OnDestroy(ref SystemState state) {
			MasterDataLoader.DisposeAllTable();
		}
	}
}