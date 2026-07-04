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
		/// Componentをまとめてセット
		/// </summary>
		public static void SetComponents<T1, T2>(
			EntityManager entityManager, 
			Entity entity, 
			T1 component1, 
			T2 component2
		)
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
		{
			entityManager.SetComponentData(entity, component1);
			entityManager.SetComponentData(entity, component2);
		}

		/// <summary>
		/// Componentをまとめてセット
		/// </summary>
		public static void SetComponents<T1, T2, T3>(
			EntityManager entityManager, 
			Entity entity, 
			T1 component1, 
			T2 component2,
			T3 component3
		)
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
		{
			entityManager.SetComponentData(entity, component1);
			entityManager.SetComponentData(entity, component2);
			entityManager.SetComponentData(entity, component3);
		}

		/// <summary>
		/// Componentをまとめてセット
		/// </summary>
		public static void SetComponents<T1, T2, T3, T4>(
			EntityManager entityManager, 
			Entity entity, 
			T1 component1, 
			T2 component2,
			T3 component3,
			T4 component4
		)
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData
		{
			entityManager.SetComponentData(entity, component1);
			entityManager.SetComponentData(entity, component2);
			entityManager.SetComponentData(entity, component3);
			entityManager.SetComponentData(entity, component4);
		}

				/// <summary>
		/// Componentをまとめてセット
		/// </summary>
		public static void SetComponents<T1, T2, T3, T4, T5>(
			EntityManager entityManager, 
			Entity entity, 
			T1 component1, 
			T2 component2,
			T3 component3,
			T4 component4,
			T5 component5
		)
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData
			where T5: unmanaged, IComponentData
		{
			entityManager.SetComponentData(entity, component1);
			entityManager.SetComponentData(entity, component2);
			entityManager.SetComponentData(entity, component3);
			entityManager.SetComponentData(entity, component4);
			entityManager.SetComponentData(entity, component5);
		}


		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1>(this EntityManager entityManager, T1 component1) where T1: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();

			Entity entity = entityManager.CreateEntity(type1);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			return entity;
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2>(this EntityManager entityManager, T1 component1, T2 component2) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();

			Entity entity = entityManager.CreateEntity(type1, type2);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			return entity;
		}
		
		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();
			ComponentType type3 = ComponentType.ReadWrite<T3>();

			Entity entity = entityManager.CreateEntity(type1, type2, type3);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			if (!type3.IsZeroSized) entityManager.SetComponentData(entity, component3);
			return entity;
		}

				
		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3, T4>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3, T4 component4) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();
			ComponentType type3 = ComponentType.ReadWrite<T3>();
			ComponentType type4 = ComponentType.ReadWrite<T4>();

			Entity entity = entityManager.CreateEntity(type1, type2, type3, type4);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			if (!type3.IsZeroSized) entityManager.SetComponentData(entity, component3);
			if (!type4.IsZeroSized) entityManager.SetComponentData(entity, component4);
			return entity;
		}

				/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3, T4, T5>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData
			where T5: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();
			ComponentType type3 = ComponentType.ReadWrite<T3>();
			ComponentType type4 = ComponentType.ReadWrite<T4>();
			ComponentType type5 = ComponentType.ReadWrite<T5>();

			Entity entity = entityManager.CreateEntity(type1, type2, type3, type4, type5);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			if (!type3.IsZeroSized) entityManager.SetComponentData(entity, component3);
			if (!type4.IsZeroSized) entityManager.SetComponentData(entity, component4);
			if (!type5.IsZeroSized) entityManager.SetComponentData(entity, component5);
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