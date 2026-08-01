using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class HexegeerCharacterBehaviour : MonoBehaviour {
		private Entity observeEntity;
		private string resourceAddress;

		public void OnSpawn(Entity observeEntity, string resourceAddress) {
			this.observeEntity = observeEntity;
			this.resourceAddress = resourceAddress;
		}

		private void LateUpdate() {
			if (observeEntity != Entity.Null) {
				EntityManager entityManager = ECS.EntityManager;
				if (entityManager.Exists(observeEntity)) {
					LocalToWorld localToWorld = entityManager.GetComponentData<LocalToWorld>(observeEntity);
					transform.SetPositionAndRotation(localToWorld.Position, localToWorld.Rotation);
				} else {
					Destroy(gameObject);
					enabled = false;
				}
			}
		}

		private void OnDestroy() {
			AssetUtil.Release(resourceAddress);
		}
	}
}