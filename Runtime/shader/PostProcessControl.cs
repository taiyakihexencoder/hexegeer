using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace hexegeer {
	public sealed class PostProcessControl : MonoBehaviour {
		private static PostProcessControl _instance;
		private VolumeProfile _profile;

		private List<PostProcessRunnerHolder> _runnerHolders;

		private void Awake() {
			_instance = this;
			_profile = GetComponent<Volume>()?.profile;
			_runnerHolders = new List<PostProcessRunnerHolder>();
		}

		private void Update() {
			float elapsed = Time.realtimeSinceStartup;

			for (int i = _runnerHolders.Count-1; i >= 0; --i) {
				if (_runnerHolders[i].Unused) {
					_runnerHolders.RemoveAt(i);
				} else {
					_runnerHolders[i].Update(elapsed);
				}
			}
		}

		public static bool TryGetProfile<T>(out T volume) where T: VolumeComponent {
			return _instance._profile.TryGet(out volume);
		}

		public static void RegisterProcess(IPostProcessRunner runner) {
			_instance._runnerHolders.Add(new PostProcessRunnerHolder(runner));
		}

		private class PostProcessRunnerHolder {
			private float _startTime;
			private float _offset;
			private IPostProcessRunner _runner;

			public bool Unused => _runner.Unused;

			public PostProcessRunnerHolder(IPostProcessRunner runner) {
				_startTime = Time.realtimeSinceStartup;
				_offset = 0f;
				_runner = runner;
				_runner.Setup();
			}

			public void Update(float time) {
				_runner.Update(time - _startTime + _offset);
			}
		}
	}
}