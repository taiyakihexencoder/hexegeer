using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerDamageObjectSystemGroup))]
	public partial struct DamageObjectCalculateSystem : ISystem {
		private EntityQuery _query;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<DamageObjectHit>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			state.Dependency = new CalculateJob {
				
			}.ScheduleParallel(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct CalculateJob : IJobEntity {
			void Execute([EntityIndexInQuery] int sortKey, Entity entity, ref DynamicBuffer<DamageObjectHit> hits) {
				if (!hits.IsEmpty) {
					foreach(DamageObjectHit hit in hits) {
						D.Log("HIT");
					}
					hits.Clear();
				}
			}
		}
	}
}
