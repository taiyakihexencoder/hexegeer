using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerInternalSystemGroup))]
	public partial struct TreeSystem : ISystem {
		private EntityQuery _query;
		private Entity _rootEntity;

		void ISystem.OnCreate(ref SystemState state) {
			_rootEntity = state.EntityManager.CreateEntity(
				state.EntityManager.CreateArchetype(
					new ComponentType[] {
						ComponentType.ReadWrite(typeof(LocalTransform)),
						ComponentType.ReadWrite(typeof(LocalToWorld)),
					}
				)
			);
			ECS.SetEntityName(state.EntityManager, _rootEntity, "Root@Hexegeer");

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<AttachHexegeerTree>()
				.WithAllRW<Parent>()
				.Build(ref state);
			state.RequireForUpdate(_query);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			state.Dependency = new ExecuteJob {
				commandBuffer = CreateCommandBuffer(ref state).AsParallelWriter(),
				root = _rootEntity,
			}.ScheduleParallel(_query, state.Dependency);
		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		partial struct ExecuteJob : IJobEntity {
			public EntityCommandBuffer.ParallelWriter commandBuffer;
			public Entity root;

			void Execute(in Entity entity, [EntityIndexInQuery] int sortKey, RefRW<Parent> parent) {
				parent.ValueRW.Value = root;
				commandBuffer.RemoveComponent<AttachHexegeerTree>(sortKey, entity);
			}
		}
	}

}
