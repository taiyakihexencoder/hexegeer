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
	}
}