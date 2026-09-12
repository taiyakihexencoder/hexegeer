using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerEventSystemGroup))]
	public partial class AdvSystem : SystemBase {
		private string _scriptAddress = "";
		private AdvScript _script = null;
		private AdvPlayer _player = null;

		private EntityQuery _query;

		protected override void OnCreate() {
			_player = new AdvPlayer();

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<AdvPlay>()
				.Build(EntityManager);
			RequireForUpdate(_query);
		}

		protected override void OnStartRunning() {
			foreach(RefRO<AdvPlay> advPlay in SystemAPI.Query<RefRO<AdvPlay>>()) {
				Task.Run(async () => {
					_player.Reset();
					string address = advPlay.ValueRO.address.ToString();
					_scriptAddress = address;
					_script = await AssetUtil.RequestLoad<AdvScript>(address);
					SyncContext.Send(() => {
						_player.Start(_script);
					});
				});
				break;
			}
		}

		protected override void OnStopRunning() {
			AssetUtil.Release(_scriptAddress);
			_scriptAddress = "";
		}

		protected override void OnUpdate() {
			if (SystemAPI.TryGetSingletonEntity<InputMainStick>(out Entity inputEntity)){
				DynamicBuffer<InputReleasedEvent> releasedEvent = SystemAPI.GetBuffer<InputReleasedEvent>(inputEntity);
				RefRO<InputMainStick> mainStick = SystemAPI.GetComponentRO<InputMainStick>(inputEntity);

				if (_script != null) {
					if (_player.FlagEnd) {
						_script = null;
						EntityCommandBuffer commandBuffer = CreateCommandBuffer();
						commandBuffer.DestroyEntity(_query, EntityQueryCaptureMode.AtPlayback);
					} else {
						_player.Update(_script, releasedEvent, mainStick.ValueRO);
					}
				}
			}
		}

		private EntityCommandBuffer CreateCommandBuffer() {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(World.Unmanaged);
		}
	}
}