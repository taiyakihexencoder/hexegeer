using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerCharacterSystemGroup))]
	public partial struct CharacterSpawnSystem : ISystem {
		private EntityQuery _requestQuery;
		private EntityQuery _prefabQuery;

		void ISystem.OnCreate(ref SystemState state) {
			_requestQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterSpawnRequest>()
				.Build(ref state);
			state.RequireForUpdate(_requestQuery);

			_prefabQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.WithAll<CharacterPrefabEntry>()
				.Build(ref state);
			state.RequireForUpdate(_prefabQuery);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			EntityCommandBuffer.ParallelWriter commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter();
			NativeArray<CharacterPrefabEntry> entries = _prefabQuery.ToComponentDataArray<CharacterPrefabEntry>(Allocator.TempJob);

			state.Dependency = new SpawnJob {
				commandBuffer = commandBuffer,
				entries = entries,
			}.ScheduleParallel(_requestQuery, state.Dependency);

			state.Dependency = entries.Dispose(state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct SpawnJob : IJobEntity {
			public EntityCommandBuffer.ParallelWriter commandBuffer;

			[ReadOnly]
			public NativeArray<CharacterPrefabEntry> entries;

			void Execute([EntityIndexInQuery]int sortKey, in Entity entity, RefRO<CharacterSpawnRequest> request) {
				for (int i = 0; i < entries.Length; ++i) {
					if (entries[i].id == request.ValueRO.id) {
						Entity instance = commandBuffer.Instantiate(sortKey, entries[i].prefab);
						commandBuffer.SetComponent(
							sortKey, 
							instance, 
							LocalTransform.FromPositionRotation(request.ValueRO.position, request.ValueRO.rotation)
						);
						commandBuffer.SetComponent(
							sortKey,
							instance,
							new LocalToWorld { Value = float4x4.TRS(request.ValueRO.position, request.ValueRO.rotation, new float3(1f,1f,1f))}
						);
						commandBuffer.RemoveComponent<Parent>(sortKey, entity);
						commandBuffer.DestroyEntity(sortKey, entity);
						break;
					}
				}
			}
		}
	}
}
