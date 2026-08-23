using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

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
				commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter(),
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
			public EntityCommandBuffer.ParallelWriter commandBuffer;

			void Execute([EntityIndexInQuery] int sortKey, Entity entity, ref DynamicBuffer<DamageObjectHit> hits) {
				if (!hits.IsEmpty) {
					Entity oscillate = commandBuffer.CreateEntity(sortKey);
					commandBuffer.AddComponent(sortKey, oscillate, CameraOscillationRequest.Once(new float3(0f, 1f, 0f), 0.5f, 0.5f));

					foreach(DamageObjectHit hit in hits) {
						D.Log("HIT");
					}
					hits.Clear();
				}
			}
		}
	}
}
