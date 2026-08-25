using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	/// <summary>
	/// contentKeyが一致するイベント領域を破棄する
	/// </summary>
	[UpdateInGroup(typeof(HexegeerEventPointSystemGroup))]
	public partial struct EventPointDeleteSystem : ISystem {
		private EntityQuery _query;

		private EntityQuery _requestQuery;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<EventPoint>()
				.Build(ref state);
			state.RequireForUpdate(_query);

			_requestQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<EventPointDeleteRequest>()
				.Build(ref state);
			state.RequireForUpdate(_requestQuery);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			NativeArray<EventPointDeleteRequest> requests = _requestQuery.ToComponentDataArray<EventPointDeleteRequest>(Allocator.TempJob);
			EntityCommandBuffer commandBuffer = CreateCommandBuffer(ref state);
			state.Dependency = new DeleteJob {
				requests = requests,
				commandBuffer = commandBuffer.AsParallelWriter()
			}.ScheduleParallel(_query, state.Dependency);
			state.Dependency = requests.Dispose(state.Dependency);
			commandBuffer.DestroyEntity(_query, EntityQueryCaptureMode.AtPlayback);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct DeleteJob : IJobEntity {
			[ReadOnly] public NativeArray<EventPointDeleteRequest> requests;
			public EntityCommandBuffer.ParallelWriter commandBuffer;

			void Execute([EntityIndexInQuery] int sortKey, in Entity entity, RefRO<EventPoint> eventPoint) {
				foreach (EventPointDeleteRequest request in requests) {
					if (request.contentKey == eventPoint.ValueRO.contentKey) {
						commandBuffer.DestroyEntity(sortKey, entity);
						break;
					}
				}
			}
		}
	}
}
