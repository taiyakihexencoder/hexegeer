using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerDamageObjectSystemGroup))]
	public partial struct DamageObjectSpawnSystem : ISystem {
		private EntityQuery _requestQuery;
		private EntityQuery _entryQuery;

		void ISystem.OnCreate(ref SystemState state) {
			_requestQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<DamageObjectSpawnRequest>()
				.Build(ref state);
			state.RequireForUpdate(_requestQuery);

			_entryQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<DamageObjectPrefabEntry>()
				.Build(ref state);
			state.RequireForUpdate(_entryQuery);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			EntityCommandBuffer.ParallelWriter commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter();
			NativeArray<DamageObjectPrefabEntry> entries = _entryQuery.ToComponentDataArray<DamageObjectPrefabEntry>(Allocator.TempJob);

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
			public NativeArray<DamageObjectPrefabEntry> entries;

			void Execute([EntityIndexInQuery] int sortKey, in Entity entity, RefRO<DamageObjectSpawnRequest> request) {
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
						commandBuffer.RemoveComponent<Parent>(sortKey, instance);

						if (request.ValueRO.owner == Entity.Null) {
							commandBuffer.RemoveComponent<EntityOwner>(sortKey, instance);
						} else {
							commandBuffer.SetComponent(sortKey, instance, new EntityOwner { owner = request.ValueRO.owner });
						}

						commandBuffer.DestroyEntity(sortKey, entity);
						break;
					}
				}
			}
		}
	}
}
