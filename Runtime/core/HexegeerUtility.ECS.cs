using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Entities;

namespace hexegeer {
	public static partial class HexegeerUtility {
		public static class ECS {
			/// <summary>
			/// Entityを作成する。
			/// </summary>
			public static Entity CreateEntity<T1>(T1 component1) where T1: unmanaged, IComponentData {
				return internallib.ECS.CreateEntity(component1);
			}

			/// <summary>
			/// Entityを作成する。
			/// </summary>
			public static Entity CreateEntity<T1, T2>(T1 component1, T2 component2) 
				where T1: unmanaged, IComponentData 
				where T2: unmanaged, IComponentData {
				return internallib.ECS.CreateEntity(component1, component2);
			}

			/// <summary>
			/// Entityを作成する。
			/// </summary>
			public static Entity CreateEntity<T1, T2, T3>(T1 component1, T2 component2, T3 component3) 
				where T1: unmanaged, IComponentData 
				where T2: unmanaged, IComponentData 
				where T3: unmanaged, IComponentData {
				return internallib.ECS.CreateEntity(component1, component2, component3);
			}

			public static async Task WaitQueryExists(EntityQuery query) {
				while (SyncContext.Send(() => query.IsEmpty)) {
					await Task.Yield();
				}
			}

			public static async Task WaitQueryEmpty(EntityQuery query) {
				while (SyncContext.Send(() => !query.IsEmpty)) {
					await Task.Yield();
				}
			}

		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1>(this EntityManager entityManager, T1 component1) where T1: unmanaged, IComponentData {
			return internallib.ECS.Create(entityManager, component1);
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2>(this EntityManager entityManager, T1 component1, T2 component2) 
			where T1: unmanaged, IComponentData 
			where T2: unmanaged, IComponentData {
			return internallib.ECS.Create(entityManager, component1, component2);
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3) 
			where T1: unmanaged, IComponentData 
			where T2: unmanaged, IComponentData 
			where T3: unmanaged, IComponentData {
			return internallib.ECS.Create(entityManager, component1, component2, component3);
		}

		/// <summary>
		/// Entityを作成する。
		/// </summary>
		public static Entity Create<T1, T2, T3, T4>(this EntityManager entityManager, T1 component1, T2 component2, T3 component3, T4 component4) 
			where T1: unmanaged, IComponentData 
			where T2: unmanaged, IComponentData 
			where T3: unmanaged, IComponentData
			where T4: unmanaged, IComponentData {
			return internallib.ECS.Create(entityManager, component1, component2, component3, component4);
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
			return internallib.ECS.Create(entityManager, component1, component2, component3, component4, component5);
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
			return internallib.ECS.Create(entityManager, component1, component2, component3, component4, component5, component6);
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
			return internallib.ECS.Create(entityManager, component1, component2, component3, component4, component5, component6, component7);
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
			return internallib.ECS.Create(entityManager, component1, component2, component3, component4, component5, component6, component7, component8);
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
			return internallib.ECS.Create(entityManager, component1, component2, component3, component4, component5, component6, component7, component8, component9);
		}
	}
}