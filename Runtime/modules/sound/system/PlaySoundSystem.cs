using hexegeer.internallib;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerSoundModuleSystemGroup))]
	public partial class PlaySoundSystem : SystemBase {
		private EntityQuery _query;

		protected override void OnCreate() {
			base.OnCreate();

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<PlaySoundRequest>()
				.Build(EntityManager);
			RequireForUpdate(_query);
		}

		protected override void OnUpdate() {
			NativeArray<PlaySoundRequest> requests = _query.ToComponentDataArray<PlaySoundRequest>(Allocator.Temp);
			foreach(PlaySoundRequest request in requests) {
				Request(request.type, request.id);
			}
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
		private void Request(SoundType type, int id) {
			switch(type) {
				case SoundType.SystemSound: {
					HexegeerSoundModule.SoundPlayer.PlaySystemSound(id);
					break;
				}
				case SoundType.Environment: {
					HexegeerSoundModule.SoundPlayer.PlayEnvironmentSound(id);
					break;
				}
			}
		}
	}
}
