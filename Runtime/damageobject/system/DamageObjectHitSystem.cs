using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerUseColliderGroup))]
	public partial struct DamageObjectHitSystem : ISystem {
		private EntityQuery _query;
		private BufferLookup<DamageObjectHit> _damageObjectHitLookup;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<ColliderTriggerEnterEvent, DamageObjectControl>()
				.Build(ref state);
			state.RequireForUpdate(_query);
			_damageObjectHitLookup = state.GetBufferLookup<DamageObjectHit>(true);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			_damageObjectHitLookup.Update(ref state);
			EntityCommandBuffer.ParallelWriter commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter();

			
			state.Dependency = new HitJob {
				commandBuffer = commandBuffer,
				damageObjectHitLookup = _damageObjectHitLookup,
			}.ScheduleParallel(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct HitJob : IJobEntity {
			public EntityCommandBuffer.ParallelWriter commandBuffer;
			[ReadOnly] public BufferLookup<DamageObjectHit> damageObjectHitLookup;

			void Execute([EntityIndexInQuery] int sortKey, ref DynamicBuffer<ColliderTriggerEnterEvent> evts) {
				foreach(ColliderTriggerEnterEvent evt in evts) {
					if (damageObjectHitLookup.HasBuffer(evt.Other)) {
						commandBuffer.AppendToBuffer(sortKey, evt.Other, new DamageObjectHit{});
					}
				}
			}
		}
	}
}
