using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace hexegeer.internallib {
	public sealed class HexegeerCharacterBehaviour : MonoBehaviour {
		private Entity _observeEntity;
		private int _id;
		private ICharacterAnimationControl _animationControl;

		private void Awake() {
			_animationControl = GetComponentInChildren<ICharacterAnimationControl>(true);
		}

		public void OnSpawn(
			Entity observeEntity, 
			int id,
			in AnimationClip[] overrideClips,
			in AnimationClip[] additiveClips,
			in AnimationClip[] baseClips
		) {
			_observeEntity = observeEntity;
			_id = id;

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
			if (CharacterModelLookup.TryGetProfile(_id, out CharacterTable.ModelProfile profile)) {
				foreach(string clipAddress in profile.overrideAnimations) {
					AssetUtil.Release(clipAddress);
				}
				foreach(string clipAddress in profile.additiveAnimations) {
					AssetUtil.Release(clipAddress);
				}
				foreach(string clipAddress in profile.baseAnimations) {
					AssetUtil.Release(clipAddress);
				}
				AssetUtil.Release(profile.modelAsset);
			}
		}
	}
}