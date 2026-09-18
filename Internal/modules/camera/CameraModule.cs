using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerWorldModuleSystemGroup))]
	public partial class HexegeerCameraModuleSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerCameraModuleTag>();
		}
	}

	[UpdateInGroup(typeof(HexegeerWorldModuleAfterPhysicsSystemGroup))]
	public partial class HexegeerCameraModuleAfterPhysicsSystemGroup : ComponentSystemGroup {
		protected override void OnCreate(){
			base.OnCreate();
			RequireForUpdate<HexegeerCameraModuleTag>();
		}
	}
	
	public struct HexegeerCameraModuleTag : IComponentData { }

	public class HexegeerCameraModuleInternal : HexegeerModuleInternal { 
		private Entity _cameraEntity;
		public Entity CameraEntity => _cameraEntity;

		public async Task Launch() {
			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;
				CreateCameraEntity(entityManager);
			});
			await Task.Yield();
		}

		public async Task Discard() {
			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;

				if (entityManager.Exists(_cameraEntity)) {
					entityManager.DestroyEntity(_cameraEntity);
					_cameraEntity = Entity.Null;
				}
			});
			await Task.Yield();
		}

		public async Task CreateModuleTag() {
			await CreateInstance(new HexegeerCameraModuleTag());
		}
		
		public async Task DeleteModuleTag() {
			await DeleteInstance<HexegeerCameraModuleTag>();
		}

		private void CreateCameraEntity(EntityManager entityManager) {
			_cameraEntity = entityManager.Create(
				new CameraInstance(),
				new CameraBounds(),
				new FixedCamera(),
				new FollowCamera(),
				new CameraLerp(),
				LocalTransform.Identity,
				new LocalToWorld { Value = float4x4.identity, },
				new Parent(),
				new AttachHexegeerTree()
			);
			entityManager.AddComponent<CameraOscillation>(_cameraEntity);

			entityManager.SetComponentEnabled<CameraBounds>(_cameraEntity, false);
			entityManager.SetComponentEnabled<FixedCamera>(_cameraEntity, false);
			entityManager.SetComponentEnabled<FollowCamera>(_cameraEntity, false);
			entityManager.SetComponentEnabled<CameraLerp>(_cameraEntity, false);
			ECS.SetEntityName(entityManager, _cameraEntity, "Camera@Hexegeer");
		}

	}
}
