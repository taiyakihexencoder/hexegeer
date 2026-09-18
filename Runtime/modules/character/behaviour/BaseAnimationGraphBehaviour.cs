using hexegeer.internallib;
using Unity.Entities;
using UnityEngine;

namespace hexegeer {
	public abstract class BaseAnimationGraphBehaviour : MonoBehaviour, ICharacterAnimationControl {
		private AnimationGraph _graph = null;

		[SerializeField]
		private Animator _animator;
		protected Animator Animator => _animator;

		private void Reset() {
			_animator = GetComponentInChildren<Animator>();
		}

		private void Awake() {
			_graph = new AnimationGraph(GetType().Name);
			_graph.SetTarget(_animator);
		}

		void ICharacterAnimationControl.OnSpawn(in AnimationClip[] overrideClips, in AnimationClip[] additiveClips, in AnimationClip[] baseClips) {
			_graph.SetAnimationClips(null, baseClips, overrideClips, additiveClips);
		}

		void ICharacterAnimationControl.Update(Entity observeEntity) {
			OnUpdate(observeEntity);
			_graph.Update(Time.deltaTime);
		}

		protected abstract void OnUpdate(Entity observeEntity);

		private void OnDestroy() {
			_graph.Destroy();
		}

		public void UpdateBaseWeight(int index, float weight) { _graph.UpdateBaseWeight(index, weight); }
		public void UpdateOverrideWeight(int index, float weight) { _graph.UpdateOverrideWeight(index, weight); }
		public void UpdateAdditiveWeight(int index, float weight) { _graph.UpdateAdditiveWeight(index, weight); }
	}
}