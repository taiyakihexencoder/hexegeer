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
	}
}