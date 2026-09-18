using hexegeer.internallib;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerSoundModuleSystemGroup))]
	public partial class MusicChangeSystem : SystemBase {
		private EntityQuery _query;

		protected override void OnCreate() {
			base.OnCreate();

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<PlayMusicRequest>()
				.Build(EntityManager);
			RequireForUpdate(_query);
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
			HexegeerSoundModule.MusicPlayer.RequestPlay(id);
		}
	}
}
