using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace hexegeer.internallib {
	public static class HexegeerManager {
		private static Entity _systemEntity = Entity.Null;
		private static Entity _entryPointEntity = Entity.Null;

		/// <summary>
		/// Hexegeer関連のすべてのシステムを起動可能にする
		/// </summary>
		/// <param name="entityManager"></param>
		public static void BootSystem(EntityManager entityManager) {
			if (_systemEntity == Entity.Null) {
				_systemEntity = entityManager.Create(
					new Parent(),
					LocalTransform.FromPosition(float3.zero),
					new LocalToWorld { Value = float4x4.identity, },
					new HexegeerSystemInstance{ },
					new AttachHexegeerTree { }
				);
				ECS.SetEntityName(entityManager, _systemEntity, "System@Hexegeer");
			}
		}

		/// <summary>
		/// World関連のすべてのシステムを起動可能にする
		/// </summary>
		/// <param name="entityManager"></param>
		public static void StartWorld(EntityManager entityManager) {
			if (_systemEntity == Entity.Null) {
				D.LogW("System did not boot !");
			} else {
				entityManager.AddComponent<HexegeerWorldInstance>(_systemEntity);
			}
		}

		/// <summary>
		/// Worldのシステム開始に必要なEntityをセットする
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="position"></param>
		public static void CreateEntryPoint(EntityManager entityManager, float3 position) {
			if (_entryPointEntity == Entity.Null) {
				_entryPointEntity = entityManager.Create(
					new Parent { Value = _systemEntity, },
					LocalTransform.FromPosition(position),
					new LocalToWorld { Value = float4x4.Translate(position), },
					new FieldObservationPoint{ }
				);
				ECS.SetEntityName(entityManager, _entryPointEntity, "Dummy Entry Point@Hexegeer");
			}
		}

		public static void DeleteEntryPoint(EntityManager entityManager) {
			if (_entryPointEntity != Entity.Null) {
				entityManager.DestroyEntity(_entryPointEntity);
				_entryPointEntity = Entity.Null;
			}
		}

		public static void EndWorld(EntityManager entityManager) {
			if (_systemEntity != Entity.Null) {
				entityManager.RemoveComponent<HexegeerWorldInstance>(_systemEntity);
			}
		}

		public static void ShutdownSystem(EntityManager entityManager) {
			if (_systemEntity != Entity.Null) {
				entityManager.DestroyEntity(_systemEntity);
				_systemEntity = Entity.Null;
			}
		}
	}
}