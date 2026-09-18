using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerRuntimeModuleSystemGroup))]
	public partial class HexegeerDebugModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerDebugModuleTag>();
		}
	}
	
	public struct HexegeerDebugModuleTag : IComponentData { }

	public class HexegeerDebugModuleInternal : HexegeerModuleInternal { 
		private Entity _physicsDebugEntity;

		public async Task Launch() {
			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;
				if (!entityManager.Exists(_physicsDebugEntity)) {
					_physicsDebugEntity = entityManager.Create(
						new PhysicsDebugDisplayData {
							DrawColliders = 0,
							DrawColliderEdges = 1,
						},
						new Parent(),
						new AttachHexegeerTree(),
						LocalTransform.Identity,
						new LocalToWorld{ Value = float4x4.identity, }
					);
					ECS.SetEntityName(entityManager, _physicsDebugEntity, "Physics Debug Display@Hexegeer");
				}
			});
			await Task.Yield();
		}

		public async Task Discard() {
			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;
				if (entityManager.Exists(_physicsDebugEntity)) {
					entityManager.DestroyEntity(_physicsDebugEntity);
					_physicsDebugEntity = Entity.Null;
				}
			});
			await Task.Yield();
		}

		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerDebugModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerDebugModuleTag>();
		}
	}
}
