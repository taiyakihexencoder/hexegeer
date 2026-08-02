using System.Net;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class HexegeerCharacterBehaviour : MonoBehaviour {
		private Entity _observeEntity;
		private CharacterTable.ModelProfile _profile;

		public void OnSpawn(Entity observeEntity, in CharacterTable.ModelProfile profile) {
			_observeEntity = observeEntity;
			_profile = profile;
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
			foreach(string clipAddress in _profile.overrideAnimations) {
				AssetUtil.Release(clipAddress);
			}
			foreach(string clipAddress in _profile.additiveAnimations) {
				AssetUtil.Release(clipAddress);
			}
			foreach(string clipAddress in _profile.baseAnimations) {
				AssetUtil.Release(clipAddress);
			}
			AssetUtil.Release(_profile.modelAsset);
		}
	}
}