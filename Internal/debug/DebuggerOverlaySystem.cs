using System.Collections.Generic;
using Unity.Entities;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerDebugSystemGroup))]
	public partial class DebuggerOverlaySystem : SystemBase {
		private const int BUFFER_FRAMES = 120;
		private const int UPDATE_PERIOD = 30;
		private List<double> _frames;
		private double _sum;
		private int _counter;

		private DebugProfilerUI _debugProfiler;

		protected override void OnStartRunning() {
			base.OnStartRunning();
			_frames = new List<double>();
			_sum = 0.0;
			_debugProfiler = DebugProfilerUI.Load();

			_counter = 0;
		}

		protected override void OnUpdate() {
			double dt = SystemAPI.Time.DeltaTime;
			_frames.Add(dt);
			_sum += dt;
			if (_frames.Count > BUFFER_FRAMES) {
				_sum -= _frames[0];
				_frames.RemoveAt(0);
			}

			if (_counter == UPDATE_PERIOD) {
				_counter = 0;
				double fps = BUFFER_FRAMES / _sum;
				_debugProfiler.SetFPS(fps);
			}
			_counter++;
		}
	}
}
