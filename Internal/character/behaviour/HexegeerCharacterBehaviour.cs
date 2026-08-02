using System.Net;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class HexegeerCharacterBehaviour : MonoBehaviour {
		private Entity _observeEntity;
		private CharacterTable.ModelProfile _profile;
		private ICharacterAnimationControl _animationControl;

		private void Awake() {
			_animationControl = GetComponentInChildren<ICharacterAnimationControl>(true);
		}

		public void OnSpawn(
			Entity observeEntity, 
			in CharacterTable.ModelProfile profile,
			in AnimationClip[] overrideClips,
			in AnimationClip[] additiveClips,
			in AnimationClip[] baseClips
		) {
			_observeEntity = observeEntity;
			_profile = profile;

			_animationControl?.OnSpawn(overrideClips, additiveClips, baseClips);
		}

		private void LateUpdate() {
			if (_observeEntity != Entity.Null) {
				EntityManager entityManager = ECS.EntityManager;
				if (entityManager.Exists(_observeEntity)) {
					LocalToWorld localToWorld = entityManager.GetComponentData<LocalToWorld>(_observeEntity);
					transform.SetPositionAndRotation(localToWorld.Position, localToWorld.Rotation);

					_animationControl?.Update(_observeEntity);
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