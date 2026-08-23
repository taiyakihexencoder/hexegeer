using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial class MusicChangeSystem : SystemBase {
		private EntityQuery _query;
		private MusicPlayer _player;


		protected override void OnCreate() {
			base.OnCreate();

			_player = new MusicPlayer();

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<PlayMusicRequest>()
				.Build(EntityManager);
			RequireForUpdate(_query);
		}

		protected override void OnDestroy() {
			base.OnDestroy();
		}

		protected override void OnUpdate() {
			NativeArray<PlayMusicRequest> requests = _query.ToComponentDataArray<PlayMusicRequest>(Allocator.Temp);
			Request(requests[0].id);
			requests.Dispose();

			EntityCommandBuffer commandBuffer = CreateCommandBuffer();
			commandBuffer.DestroyEntity(_query, EntityQueryCaptureMode.AtPlayback);
		}

		private EntityCommandBuffer CreateCommandBuffer() {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(World.Unmanaged);
		}


		[BurstDiscard]
		private void Request(int id) {
			_player.RequestPlay(id);
		}
	}
}
