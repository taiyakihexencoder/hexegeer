using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerRuntimeModuleSystemGroup))]
	public partial class HexegeerWorldModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerWorldModuleTag>();
		}
	}

	[UpdateInGroup(typeof(HexegeerRuntimeModuleAfterPhysicsSystemGroup))]
	public partial class HexegeerWorldModuleAfterPhysicsSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerWorldModuleTag>();
		}
	}

	[UpdateInGroup(typeof(HexegeerRuntimeModuleColliderSystemGroup))]
	public partial class HexegeerWorldModuleColliderSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerWorldModuleTag>();
		}
	}

	[UpdateInGroup(typeof(HexegeerRuntimeModuleAfterColliderSystemGroup))]
	public partial class HexegeerWorldModuleAfterColliderSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerWorldModuleTag>();
		}
	}
	
	public struct HexegeerWorldModuleTag : IComponentData { }

	public class HexegeerWorldModuleInternal : HexegeerModuleInternal { 
		private Entity _masterDataEntity;
		public Entity MasterDataEntity => _masterDataEntity;

		private Entity _physicsEntity;
		public Entity PhysicsEntity => _physicsEntity;

		public async Task Launch() {
			SyncContext.Send(() => {
				ECS.SetPhysicsSystemEnabled(false);

				EntityManager entityManager = ECS.EntityManager;
				_masterDataEntity = entityManager.Create(
					new Parent(),
					new LocalToWorld{ Value = float4x4.identity, },
					LocalTransform.Identity,
					new AttachHexegeerTree()
				);
				ECS.SetEntityName(entityManager, _masterDataEntity, "Master Data@Hexegeer");
			});
			await Task.Yield();
		}

		public async Task Discard() {
			SyncContext.Post(() => {
				EndPhysics();

				EntityManager entityManager = ECS.EntityManager;
				if (entityManager.Exists(_masterDataEntity)) {
					entityManager.DestroyEntity(_masterDataEntity);
					_masterDataEntity = Entity.Null;
				}
			});
			await Task.Yield();
		}

		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerWorldModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerWorldModuleTag>();
		}

		public void StartPhysics() {
			EntityManager entityManager = ECS.EntityManager;
			if (!entityManager.Exists(_physicsEntity)) {
				_physicsEntity = entityManager.Create(
					new PhysicsStep {
						SimulationType = SimulationType.UnityPhysics,
						Gravity = new float3(0.0f, -9.81f, 0.0f),
						SolverIterationCount = 4,
						SubstepCount = 1,
						MultiThreaded = 1,
					}
				);
				ECS.SetEntityName(entityManager, _physicsEntity, "Physics Step@Hexegeer");

				ECS.SetPhysicsSystemEnabled(true);
			}
		}

		public void EndPhysics() {
			EntityManager entityManager = ECS.EntityManager;
			if (entityManager.Exists(_physicsEntity)) {
				entityManager.DestroyEntity(_physicsEntity);
				_physicsEntity = Entity.Null;
			}

			ECS.SetPhysicsSystemEnabled(false);
		}
	}
}
