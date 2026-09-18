using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	public abstract class HexegeerModuleInternal {
		protected bool _isActive;
		public bool IsActive {
			get => _isActive;
			set => _isActive = value;
		}

		public HexegeerModuleInternal() {
			_isActive = false;
		}

		protected async Task CreateInstance<T>(T component) where T : unmanaged, IComponentData {
			SyncContext.Post(() => {
				EntityManager entityManager = ECS.EntityManager;
				Entity entity = entityManager.Create(component);
				ECS.SetEntityName(entityManager, entity, $"{typeof(T).Name}@Hexegeer");
			});
			await Task.Yield();
		}

		protected async Task DeleteInstance<T>() where T : unmanaged, IComponentData {
			SyncContext.Send(() => {
				EntityManager entityManager = ECS.EntityManager;
				EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
					.WithAll<T>()
					.Build(entityManager);
				entityManager.DestroyEntity(query);
			});
			await Task.Yield();
		}
	}
}