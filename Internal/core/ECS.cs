using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer.internallib {
	public static class ECS {
		public static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
	
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

		public static void SetComponents<T1>(
			EntityManager entityManager, 
			Entity entity, 
			T1 component1
		)
			where T1: unmanaged, IComponentData
		{
			entityManager.SetComponentData(entity, component1);
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
		public static Entity Create<T1, T2, T3, T4, T5, T6>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData
			where T5: unmanaged, IComponentData
			where T6: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();
			ComponentType type3 = ComponentType.ReadWrite<T3>();
			ComponentType type4 = ComponentType.ReadWrite<T4>();
			ComponentType type5 = ComponentType.ReadWrite<T5>();
			ComponentType type6 = ComponentType.ReadWrite<T6>();

			Entity entity = entityManager.CreateEntity(type1, type2, type3, type4, type5, type6);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			if (!type3.IsZeroSized) entityManager.SetComponentData(entity, component3);
			if (!type4.IsZeroSized) entityManager.SetComponentData(entity, component4);
			if (!type5.IsZeroSized) entityManager.SetComponentData(entity, component5);
			if (!type6.IsZeroSized) entityManager.SetComponentData(entity, component6);
			return entity;
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3, T4, T5, T6, T7>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData
			where T5: unmanaged, IComponentData
			where T6: unmanaged, IComponentData
			where T7: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();
			ComponentType type3 = ComponentType.ReadWrite<T3>();
			ComponentType type4 = ComponentType.ReadWrite<T4>();
			ComponentType type5 = ComponentType.ReadWrite<T5>();
			ComponentType type6 = ComponentType.ReadWrite<T6>();
			ComponentType type7 = ComponentType.ReadWrite<T7>();

			Entity entity = entityManager.CreateEntity(type1, type2, type3, type4, type5, type6, type7);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			if (!type3.IsZeroSized) entityManager.SetComponentData(entity, component3);
			if (!type4.IsZeroSized) entityManager.SetComponentData(entity, component4);
			if (!type5.IsZeroSized) entityManager.SetComponentData(entity, component5);
			if (!type6.IsZeroSized) entityManager.SetComponentData(entity, component6);
			if (!type7.IsZeroSized) entityManager.SetComponentData(entity, component7);
			return entity;
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3, T4, T5, T6, T7, T8>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData
			where T5: unmanaged, IComponentData
			where T6: unmanaged, IComponentData
			where T7: unmanaged, IComponentData
			where T8: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();
			ComponentType type3 = ComponentType.ReadWrite<T3>();
			ComponentType type4 = ComponentType.ReadWrite<T4>();
			ComponentType type5 = ComponentType.ReadWrite<T5>();
			ComponentType type6 = ComponentType.ReadWrite<T6>();
			ComponentType type7 = ComponentType.ReadWrite<T7>();
			ComponentType type8 = ComponentType.ReadWrite<T8>();

			Entity entity = entityManager.CreateEntity(type1, type2, type3, type4, type5, type6, type7, type8);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			if (!type3.IsZeroSized) entityManager.SetComponentData(entity, component3);
			if (!type4.IsZeroSized) entityManager.SetComponentData(entity, component4);
			if (!type5.IsZeroSized) entityManager.SetComponentData(entity, component5);
			if (!type6.IsZeroSized) entityManager.SetComponentData(entity, component6);
			if (!type7.IsZeroSized) entityManager.SetComponentData(entity, component7);
			if (!type8.IsZeroSized) entityManager.SetComponentData(entity, component8);
			return entity;
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3, T4 component4, T5 component5, T6 component6, T7 component7, T8 component8, T9 component9) 
			where T1: unmanaged, IComponentData
			where T2: unmanaged, IComponentData
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData
			where T5: unmanaged, IComponentData
			where T6: unmanaged, IComponentData
			where T7: unmanaged, IComponentData
			where T8: unmanaged, IComponentData
			where T9: unmanaged, IComponentData {
			ComponentType type1 = ComponentType.ReadWrite<T1>();
			ComponentType type2 = ComponentType.ReadWrite<T2>();
			ComponentType type3 = ComponentType.ReadWrite<T3>();
			ComponentType type4 = ComponentType.ReadWrite<T4>();
			ComponentType type5 = ComponentType.ReadWrite<T5>();
			ComponentType type6 = ComponentType.ReadWrite<T6>();
			ComponentType type7 = ComponentType.ReadWrite<T7>();
			ComponentType type8 = ComponentType.ReadWrite<T8>();
			ComponentType type9 = ComponentType.ReadWrite<T9>();

			Entity entity = entityManager.CreateEntity(type1, type2, type3, type4, type5, type6, type7, type8, type9);
			if (!type1.IsZeroSized) entityManager.SetComponentData(entity, component1);
			if (!type2.IsZeroSized) entityManager.SetComponentData(entity, component2);
			if (!type3.IsZeroSized) entityManager.SetComponentData(entity, component3);
			if (!type4.IsZeroSized) entityManager.SetComponentData(entity, component4);
			if (!type5.IsZeroSized) entityManager.SetComponentData(entity, component5);
			if (!type6.IsZeroSized) entityManager.SetComponentData(entity, component6);
			if (!type7.IsZeroSized) entityManager.SetComponentData(entity, component7);
			if (!type8.IsZeroSized) entityManager.SetComponentData(entity, component8);
			if (!type9.IsZeroSized) entityManager.SetComponentData(entity, component9);
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

		public static void RemoveComponents<T1, T2>(this EntityManager entityManager, Entity entity) 
			where T1: unmanaged, IComponentData 
			where T2: unmanaged, IComponentData {
			entityManager.RemoveComponent<T1>(entity);
			entityManager.RemoveComponent<T2>(entity);
		}

		public static void RemoveComponents<T1, T2, T3>(this EntityManager entityManager, Entity entity) 
			where T1: unmanaged, IComponentData 
			where T2: unmanaged, IComponentData 
			where T3: unmanaged, IComponentData {
			entityManager.RemoveComponent<T1>(entity);
			entityManager.RemoveComponent<T2>(entity);
			entityManager.RemoveComponent<T3>(entity);
		}

		public static void RemoveComponents<T1, T2, T3, T4>(this EntityManager entityManager, Entity entity) 
			where T1: unmanaged, IComponentData 
			where T2: unmanaged, IComponentData 
			where T3: unmanaged, IComponentData 
			where T4: unmanaged, IComponentData {
			entityManager.RemoveComponent<T1>(entity);
			entityManager.RemoveComponent<T2>(entity);
			entityManager.RemoveComponent<T3>(entity);
			entityManager.RemoveComponent<T4>(entity);
		}
	}
}