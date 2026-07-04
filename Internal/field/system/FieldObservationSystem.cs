using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer.internallib {
	/// <summary>
	/// フィールドの読み込みと破棄の判定を行う。
	/// </summary>
	[UpdateInGroup(typeof(HexegeerFieldInternalSystemGroup))]
	public partial struct FieldObservationSystem : ISystem {
		private EntityQuery _observerQuery;
		private EntityQuery _fieldQuery;

		void ISystem.OnCreate(ref SystemState state) {
			_observerQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<FieldObservationPoint, LocalToWorld>()
				.Build(ref state);
			state.RequireForUpdate(_observerQuery);

			_fieldQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<Child>()
				.WithAllRW<FieldHeader>()
				.Build(ref state);
			state.RequireForUpdate(_fieldQuery);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			if (!SystemAPI.TryGetSingleton(out FieldSetting setting)) { return; }

			NativeArray<LocalToWorld> localToWorlds = _observerQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);

			EntityCommandBuffer.ParallelWriter commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter();

			state.Dependency = new CheckLoadJob {
				localToWorlds = localToWorlds,
				elapsed = SystemAPI.Time.ElapsedTime,
				commandBuffer = commandBuffer,
				loadDistance = setting.loadFieldDistance,
			}.ScheduleParallel(_fieldQuery, state.Dependency);

			state.Dependency = new CheckUnloadJob {
				localToWorlds = localToWorlds,
				elapsed = SystemAPI.Time.ElapsedTime,
				commandBuffer = commandBuffer,
				unloadDistance = setting.unloadFieldDistance,
			}.ScheduleParallel(_fieldQuery, state.Dependency);

			state.Dependency = localToWorlds.Dispose(state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct CheckLoadJob : IJobEntity {
			[ReadOnly]
			public NativeArray<LocalToWorld> localToWorlds;

			[ReadOnly]
			public float loadDistance;

			[ReadOnly]
			public double elapsed;

			[ReadOnly]
			public double updateInterval;

			public EntityCommandBuffer.ParallelWriter commandBuffer;

			void Execute(in Entity entity, [EntityIndexInQuery] int sortKey, RefRW<FieldHeader> header) {

				if (header.ValueRO.active) { return; }
				if (elapsed - header.ValueRO.lastUpdated < updateInterval) { return; }

				float3 boundsMin = header.ValueRO.boundsMin;
				float3 boundsMax = header.ValueRO.boundsMax;
				float3 position;
				
				foreach(LocalToWorld localToWorld in localToWorlds) {
					position = localToWorld.Position;
					if (
						boundsMin.x - loadDistance < position.x && 
						boundsMin.y - loadDistance < position.y &&
						boundsMin.z - loadDistance < position.z &&
						position.x < boundsMax.x + loadDistance &&
						position.y < boundsMax.y + loadDistance &&
						position.z < boundsMax.z + loadDistance
					) {
						// 再リクエストを防ぐために、読み込み前にactiveフラグを立てておく
						header.ValueRW.active = true;
						header.ValueRW.lastUpdated = elapsed;

						Entity requestEntity = commandBuffer.CreateEntity(sortKey);
						commandBuffer.AddComponent(
							sortKey, 
							requestEntity, 
							new FieldLoadRequest { id = header.ValueRO.id, }
						);
						break;
					}
				}
			}
		}

		partial struct CheckUnloadJob : IJobEntity {
			[ReadOnly]
			public NativeArray<LocalToWorld> localToWorlds;

			[ReadOnly]
			public float unloadDistance;

			[ReadOnly]
			public double elapsed;

			[ReadOnly]
			public double updateInterval;

			public EntityCommandBuffer.ParallelWriter commandBuffer;

			void Execute(
				in Entity entity, 
				[EntityIndexInQuery] int sortKey, 
				RefRW<FieldHeader> header,
				ref DynamicBuffer<Child> children
			) {
				if (!header.ValueRO.active) { return; }
				if (elapsed - header.ValueRO.lastUpdated < updateInterval) { return; }

				float3 boundsMin = header.ValueRO.boundsMin;
				float3 boundsMax = header.ValueRO.boundsMax;
				float3 position;
				
				foreach(LocalToWorld localToWorld in localToWorlds) {
					position = localToWorld.Position;

					if (
						boundsMin.x - unloadDistance < position.x && 
						boundsMin.y - unloadDistance < position.y &&
						boundsMin.z - unloadDistance < position.z &&
						position.x < boundsMax.x + unloadDistance &&
						position.y < boundsMax.y + unloadDistance &&
						position.z < boundsMax.z + unloadDistance
					) {
						return;
					}
				}

				header.ValueRW.active = false;
				header.ValueRW.lastUpdated = elapsed;

				foreach(Child child in children) {
					commandBuffer.DestroyEntity(sortKey, child.Value);
				}

				// コンテンツのアンロード
				Entity unloadContentsEntity = commandBuffer.CreateEntity(sortKey);
				commandBuffer.AddComponent(sortKey, unloadContentsEntity, new ContentKeyLoadRequest{ contentKey = header.ValueRO.contentKey, });
			}
		}
	}

}
