using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerUseColliderGroup))]
	public partial struct EventPointSystem : ISystem {
		private EntityQuery _query;

		private ComponentLookup<EventPointAccessible> _accessibles;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<EventPoint, ColliderTriggerEnterEvent, ColliderTriggerExitEvent>()
				.Build(ref state);
			state.RequireForUpdate(_query);

			_accessibles = state.GetComponentLookup<EventPointAccessible>(true);

			Entity eventObserveEntity = state.EntityManager.Create(
				new Parent{},
				new AttachHexegeerTree{},
				LocalTransform.FromPosition(0f,0f,0f),
				new LocalToWorld{ Value = Unity.Mathematics.float4x4.identity, }
			);
			state.EntityManager.SetName(eventObserveEntity, "Event Elements@Hexegeer");
			state.EntityManager.AddBuffer<EventElement>(eventObserveEntity);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			_accessibles.Update(ref state);

			if (SystemAPI.TryGetSingletonBuffer(out DynamicBuffer<EventElement> eventElements) ) {
				state.Dependency = new TriggerEnterJob {
					accessibles = _accessibles,
					eventElements = eventElements,
				}.Schedule(_query, state.Dependency);

				state.Dependency = new TriggerExitJob {
					accessibles = _accessibles,
					eventElements = eventElements,
				}.Schedule(_query, state.Dependency);
			}

		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct TriggerEnterJob : IJobEntity {
			[ReadOnly] public ComponentLookup<EventPointAccessible> accessibles;
			public DynamicBuffer<EventElement> eventElements;

			void Execute(in Entity entity, RefRO<EventPoint> eventpoint, ref DynamicBuffer<ColliderTriggerEnterEvent> evts) {
				foreach (ColliderTriggerEnterEvent evt in evts) {
					if (accessibles.HasComponent(evt.Other)) {
						eventElements.Add(
							new EventElement {
								eventId = eventpoint.ValueRO.eventId,
								entity = evt.Other
							}
						);
					}
				}
			}
		}

		partial struct TriggerExitJob : IJobEntity {
			[ReadOnly] public ComponentLookup<EventPointAccessible> accessibles;
			public DynamicBuffer<EventElement> eventElements;

			void Execute(in Entity entity, RefRO<EventPoint> eventpoint, ref DynamicBuffer<ColliderTriggerExitEvent> evts) {
				foreach (ColliderTriggerExitEvent evt in evts) {
					if (accessibles.HasComponent(evt.Other)) {
						for (int i = 0; i < eventElements.Length; ++i) {
							if (eventElements[i].eventId == eventpoint.ValueRO.eventId &&
								eventElements[i].entity == evt.Other ) {
								eventElements.RemoveAt(i);
								break;
							}
						}
					}
				}
			}
		}
	}
}
