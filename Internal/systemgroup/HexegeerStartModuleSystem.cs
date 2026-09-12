using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerSimulationSystemGroup))]
	public partial struct HexegeerStartModuleSystem : ISystem, ISystemStartStop {
		private EntityQuery _query;

		private Entity _rootEntity;

		void ISystem.OnCreate(ref SystemState state) {
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<HexegeerStartSystemModuleRequest>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			EntityCommandBuffer commandBuffer = CreateCommandBuffer(ref state);

			state.Dependency = new Job {
				commandBuffer = commandBuffer,
				rootEntity = _rootEntity,
				existsEventEntity = SystemAPI.HasSingleton<HexegeerSystemModuleEventComponent>(),
			}.Schedule(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
			if (_rootEntity != Entity.Null) {
				state.EntityManager.DestroyEntity(_rootEntity);
				_rootEntity = Entity.Null;
			}
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		void ISystemStartStop.OnStartRunning(ref SystemState state) {
			if (_rootEntity == Entity.Null) {
				_rootEntity = ECS.Create(
					state.EntityManager, 
					LocalTransform.Identity,
					new LocalToWorld { Value = float4x4.identity, },
					new Parent(),
					new AttachHexegeerTree()
				);
				ECS.SetEntityName(state.EntityManager, _rootEntity, "Mode@Hexegeer");
			}
		}

		void ISystemStartStop.OnStopRunning(ref SystemState state) { }

		partial struct Job : IJobEntity {
			public EntityCommandBuffer commandBuffer;

			[ReadOnly] public Entity rootEntity;
			[ReadOnly] public bool existsEventEntity;

			void Execute(in Entity entity, RefRO<HexegeerStartSystemModuleRequest> request) {
				switch(request.ValueRO.module) {
					case HexegeerSystemModule.Event: {
						CreateEventEntity();
						break;
					}
				}

				commandBuffer.DestroyEntity(entity);
			}

			private void CreateEventEntity() {
				if (!existsEventEntity) {
					Entity entity = commandBuffer.CreateEntity();
					commandBuffer.AddComponent(entity, new Parent { Value = rootEntity, });
					ECS.SetEntityName(commandBuffer, entity, "Event Mode");
					commandBuffer.AddComponent(entity, new HexegeerSystemModuleEventComponent{});
				}
			}
		}
	}
}
