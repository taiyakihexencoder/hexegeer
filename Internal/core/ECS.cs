using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	public static class ECS {
		[Conditional("UNITY_EDITOR")]
		public static void SetEntityName(EntityManager manager, Entity entity, FixedString64Bytes name) {
#if UNITY_EDITOR
			manager.SetName(entity, name);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void SetEntityName(EntityCommandBuffer commandBuffer, Entity entity, FixedString64Bytes name) {
#if UNITY_EDITOR
			commandBuffer.SetName(entity, name);
#endif
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1>(this EntityManager entityManager, T1 component1) where T1: unmanaged, IComponentData {
			Entity entity = entityManager.CreateEntity(
				ComponentType.ReadWrite<T1>()
			);
			entityManager.SetComponentData(entity, component1);
			return entity;
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2>(this EntityManager entityManager, T1 component1, T2 component2) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData {
			Entity entity = entityManager.CreateEntity(
				ComponentType.ReadWrite<T1>(),
				ComponentType.ReadWrite<T2>()
			);
			entityManager.SetComponentData(entity, component1);
			entityManager.SetComponentData(entity, component2);
			return entity;
		}
		
		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData {
			Entity entity = entityManager.CreateEntity(
				ComponentType.ReadWrite<T1>(),
				ComponentType.ReadWrite<T2>(),
				ComponentType.ReadWrite<T3>()
			);
			entityManager.SetComponentData(entity, component1);
			entityManager.SetComponentData(entity, component2);
			entityManager.SetComponentData(entity, component3);
			return entity;
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity CreateEntity<T1>(T1 component1) where T1: unmanaged, IComponentData {
			return World.DefaultGameObjectInjectionWorld.EntityManager.Create(component1);
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity CreateEntity<T1, T2>(T1 component1, T2 component2) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData {
			return World.DefaultGameObjectInjectionWorld.EntityManager.Create(component1, component2);
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity CreateEntity<T1, T2, T3>(T1 component1, T2 component2, T3 component3) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData {
			return World.DefaultGameObjectInjectionWorld.EntityManager.Create(component1, component2, component3);
		}
	}
}