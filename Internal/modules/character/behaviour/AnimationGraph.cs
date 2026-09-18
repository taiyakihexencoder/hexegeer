using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace hexegeer.internallib {
	using CuePlayable = UnityEngine.Playables.ScriptPlayable<hexegeer.internallib.AnimationCuePlayableBehaviour>;

	public sealed class AnimationGraph {
		private int L_BASE = 0;
		private int L_OVERRIDE = 1;
		private int L_ADDITIVE = 2;

		private PlayableGraph _graph;
		private AnimationLayerMixerPlayable _root;
		private AnimationPlayableOutput _animationOutput;

		public AnimationGraph(string name) {
			_graph = PlayableGraph.Create(name);

			_root = AnimationLayerMixerPlayable.Create(_graph, 3);
			_root.SetSpeed(1.0f);
			_root.SetLayerAdditive((uint)L_BASE, false);
			_root.SetLayerAdditive((uint)L_OVERRIDE, false);
			_root.SetLayerAdditive((uint)L_ADDITIVE, true);

			AnimationMixerPlayable baseMixer = AnimationMixerPlayable.Create(_graph);
			_graph.Connect(baseMixer, 0, _root, L_BASE);
			AnimationMixerPlayable overrideMixer = AnimationMixerPlayable.Create(_graph);
			_graph.Connect(overrideMixer, 0, _root, L_OVERRIDE);
			AnimationMixerPlayable additiveMixer = AnimationMixerPlayable.Create(_graph);
			_graph.Connect(additiveMixer, 0, _root, L_ADDITIVE);

			_root.SetInputWeight(L_BASE, 1.0f);
			_root.SetInputWeight(L_OVERRIDE, 0.0f);
			_root.SetInputWeight(L_ADDITIVE, 1.0f);

			_graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

			_animationOutput = AnimationPlayableOutput.Create(_graph, "", null);
			_animationOutput.SetSourcePlayable(_root, 0);
		}

		public void Update(float dt) {
			_graph.Evaluate(dt);
			CheckUpdateOverrideAnimation();
		}

		private void CheckUpdateOverrideAnimation() {
			if (_root.GetInputWeight(L_OVERRIDE) > float.Epsilon) {
				Playable overrideLayer = _root.GetInput(L_OVERRIDE);
				Playable mixer, clip;
				bool playing = false;
				for (int i = 0, iMax = overrideLayer.GetInputCount(); i < iMax; ++i) {
					mixer = overrideLayer.GetInput(i);
					if (mixer.IsValid() && mixer.GetPlayState() == PlayState.Playing) {
						clip = mixer.GetInput(0);
						if (clip.IsDone()) {
							mixer.Pause();
							overrideLayer.SetInputWeight(i, 0.0f);
						} else {
							playing = true;
						}
					}
				}
				if (!playing) {
					_root.SetInputWeight(L_OVERRIDE, 0.0f);
				}
			}
		}

		public void Destroy() {
			if (_graph.IsValid()) {
				_graph.Destroy();
			}
		}

		public void SetTarget(Animator animator) {
			_animationOutput.SetTarget(animator);
		}

		public void SetAnimationClips(
			IAnimationCueListener cueListener,
			in AnimationClip[] baseAnimationClips,
			in AnimationClip[] overrideAnimationClips,
			in AnimationClip[] additiveAnimationClips
		) {

			Playable baseMixer = _root.GetInput(L_BASE);
			baseMixer.SetInputCount(baseAnimationClips.Length);
			for (int i = 0; i < baseAnimationClips.Length; ++i) {
				AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(_graph, 2);
				mixer.SetInputWeight(0, 1f);
				mixer.SetInputWeight(1, 1f);
				_graph.Connect(mixer, 0, baseMixer, i);

				AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph, baseAnimationClips[i]);
				_graph.Connect(playable, 0, mixer, 0);
			}

			Playable overrideMixer = _root.GetInput(L_OVERRIDE);
			overrideMixer.SetInputCount(overrideAnimationClips.Length);
			for (int i = 0; i < overrideAnimationClips.Length; ++i) {
				AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(_graph, 2);
				mixer.SetInputWeight(0, 1f);
				mixer.SetInputWeight(1, 1f);
				_graph.Connect(mixer, 0, overrideMixer, i);

				AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph, overrideAnimationClips[i]);
				_graph.Connect(playable, 0, mixer, 0);
			}

			Playable additiveMixer = _root.GetInput(L_ADDITIVE);
			additiveMixer.SetInputCount(additiveAnimationClips.Length);
			for (int i = 0; i < additiveAnimationClips.Length; ++i) {
				AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(_graph, 2);
				mixer.SetInputWeight(0, 1f);
				mixer.SetInputWeight(1, 1f);
				_graph.Connect(mixer, 0, additiveMixer, i);

				AnimationClipPlayable playable = AnimationClipPlayable.Create(_graph, additiveAnimationClips[i]);
				_graph.Connect(playable, 0, mixer, 0);
			}
		}

		public void UpdateBaseWeight(int index, float weight) {
			_root.GetInput(L_BASE).SetInputWeight(index, weight);
		}

		public void UpdateOverrideWeight(int index, float weight) {
			_root.GetInput(L_OVERRIDE).SetInputWeight(index, weight);
		}

		public void UpdateAdditiveWeight(int index, float weight) {
			_root.GetInput(L_ADDITIVE).SetInputWeight(index, weight);
		}
	}

	public interface IAnimationCueListener {
		void Notify(int cueId, in int value);
	}

	public class AnimationCuePlayableBehaviour : PlayableBehaviour {
		private int _cueId;
		private int _signal;
		private double _previousSeconds;
		private double _cueSeconds;

		internal void Init(int cueId, int signal, double cueSeconds) {
			_cueId = cueId;
			_signal = signal;
			_cueSeconds = cueSeconds;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
			double current = playable.GetTime();
			if (_previousSeconds < _cueSeconds && _cueSeconds <= current) {
				if (playerData is IAnimationCueListener listener) {
					listener.Notify(_cueId, _signal);
				}
			}
		}
	}

}