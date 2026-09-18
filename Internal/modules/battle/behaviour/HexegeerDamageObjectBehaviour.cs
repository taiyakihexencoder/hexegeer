using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	public class HexegeerDamageObjectBehaviour : MonoBehaviour {
		private Entity _observeEntity;
		private int _id;

		public void OnSpawn(Entity observeEntity, int id) {
			_observeEntity = observeEntity;
			_id = id;
		}

		private void LateUpdate() {
			if (_observeEntity != Entity.Null) {
				EntityManager entityManager = ECS.EntityManager;
				if (entityManager.Exists(_observeEntity)) {
					LocalToWorld localToWorld = entityManager.GetComponentData<LocalToWorld>(_observeEntity);
					transform.SetPositionAndRotation(localToWorld.Position, localToWorld.Rotation);
				} else {
					Destroy(gameObject);
					enabled = false;
				}
			}
		}

		private void OnDestroy() {
			if (DamageObjectModelLookup.TryGetAssetAddress(_id, out string address)) {
				AssetUtil.Release(address);
			}
		}
	}
}
