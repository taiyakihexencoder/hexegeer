using Unity.Entities;
using UnityEngine;

namespace hexegeer.internallib {
	public interface ICharacterAnimationControl {
		void OnSpawn(
			in AnimationClip[] overrideClips,
			in AnimationClip[] additiveClips,
			in AnimationClip[] baseClips
		);

		void Update(Entity observeEntity);
	}
}