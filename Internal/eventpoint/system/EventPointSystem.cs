using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerUseColliderGroup))]
	public partial struct EventPointSystem : ISystem {
		private EntityQuery _query;

		private NativeList<int> _eventList;
		private ComponentLookup<EventPointAccessible> _accessibles;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<EventPoint, ColliderTriggerEnterEvent, ColliderTriggerExitEvent>()
				.Build(ref state);
			state.RequireForUpdate(_query);

			_eventList = new NativeList<int>(Allocator.Persistent);
			_accessibles = state.GetComponentLookup<EventPointAccessible>(true);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			_accessibles.Update(ref state);

			state.Dependency = new TriggerEnterJob {
				accessibles = _accessibles,
				eventList = _eventList,
			}.Schedule(_query, state.Dependency);

			state.Dependency = new TriggerExitJob {
				accessibles = _accessibles,
				eventList = _eventList,
			}.Schedule(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
			_eventList.Dispose();
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct TriggerEnterJob : IJobEntity {
			[ReadOnly] public ComponentLookup<EventPointAccessible> accessibles;
			public NativeList<int> eventList;

			void Execute(in Entity entity, RefRO<EventPoint> eventpoint, ref DynamicBuffer<ColliderTriggerEnterEvent> evts) {
				foreach (ColliderTriggerEnterEvent evt in evts) {
					if (accessibles.HasComponent(evt.Other)) {
						eventList.Add(eventpoint.ValueRO.eventId);
					}
				}
			}
		}

		partial struct TriggerExitJob : IJobEntity {
			[ReadOnly] public ComponentLookup<EventPointAccessible> accessibles;
			public NativeList<int> eventList;

			void Execute(in Entity entity, RefRO<EventPoint> eventpoint, ref DynamicBuffer<ColliderTriggerExitEvent> evts) {
				foreach (ColliderTriggerExitEvent evt in evts) {
					if (accessibles.HasComponent(evt.Other)) {
						for (int i = 0; i < eventList.Count; ++i) {
							if (eventList[i] == eventpoint.ValueRO.eventId) {
								eventList.RemoveAt(i);
								break;
							}
						}
					}
				}
			}
		}
	}
}
