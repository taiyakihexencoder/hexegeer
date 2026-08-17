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
				.WithAll<DamageObjectSpawnRequest, DamageObjectTrailDefinition>()
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
			public NativeArray<DamageObjectPrefabEntry> entries;

			void Execute(
				[EntityIndexInQuery] int sortKey, 
				in Entity entity, 
				RefRO<DamageObjectSpawnRequest> request,
				ref DynamicBuffer<DamageObjectTrailDefinition> definitions
			) {
				for (int i = 0; i < entries.Length; ++i) {
					if (entries[i].id == request.ValueRO.id) {
						DamageObjectCreateModelRequest modelRequest = new DamageObjectCreateModelRequest {
							id = request.ValueRO.id,
						};

						for (int n = 0; n < definitions.Length; ++n) {
							Entity instance = commandBuffer.Instantiate(sortKey, entries[i].prefab);
							commandBuffer.SetComponent(
								sortKey,
								instance,
								LocalTransform.FromPositionRotation(definitions[i].position, definitions[i].rotation)
							);
							commandBuffer.SetComponent(
								sortKey,
								instance,
								new LocalToWorld { Value = float4x4.TRS(definitions[i].position, definitions[i].rotation, new float3(1f,1f,1f)) }
							);
							commandBuffer.SetComponent(
								sortKey,
								instance,
								new LimitedLifeTime { seconds = definitions[i].limitedLifeTime, }
							);
							commandBuffer.SetComponent(
								sortKey,
								instance,
								new DamageObjectControl { 
									damageObjectId = request.ValueRO.id, 
									entityIndex = i, 
									startPosition = definitions[i].position,
									startRotation = definitions[i].rotation,
									extra = request.ValueRO.extra,
								}
							);
							commandBuffer.RemoveComponent<Parent>(sortKey, instance);

							if (request.ValueRO.owner == Entity.Null) {
								commandBuffer.RemoveComponent<EntityOwner>(sortKey, instance);
							} else {
								commandBuffer.SetComponent(sortKey, instance, new EntityOwner { owner = request.ValueRO.owner });
							}

							Entity modelRequestEntity = commandBuffer.CreateEntity(sortKey);
							modelRequest.observeEntity = instance;
							commandBuffer.AddComponent(sortKey, modelRequestEntity, modelRequest);
						}

						commandBuffer.DestroyEntity(sortKey, entity);

						break;
					}
				}
			}
		}
	}
}
