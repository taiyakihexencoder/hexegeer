using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace hexegeer {
	public sealed class AdvPlayer {
		private static IAdvProcessor _processor;

		private AdvScript.WindowSequenceProcessData _windowSequenceProcess = null;
		private AdvScript.PlayTextProcessData _playTextProcess = null;
		private AdvScript.WaitInputProcessData _waitInputProcess = null;
		private AdvScript.WaitSecondsProcessData _waitSecondsProcess = null;

		private int _sequencerIndex;
		private int _windowSequenceIndex;
		private int _playTextIndex;
		private int _waitInputIndex;
		private int _waitSecondsIndex;

		private float _waitSecondsElapsed;

		private bool _flagStart = false;
		public bool FlagStart => _flagStart;

		private bool _flagEnd = false;
		public bool FlagEnd => _flagEnd;

		public static void AssignProcessor(IAdvProcessor processor) {
			_processor = processor;
		}

		public static void Request(EntityCommandBuffer commandBuffer, FixedString64Bytes address) {
			Entity entity = commandBuffer.CreateEntity();
			commandBuffer.AddComponent(entity, new AdvPlay { address = address, });
		}

		public static void Request(EntityManager entityManager, FixedString64Bytes address) {
			entityManager.Create(new AdvPlay { address = address, });
		}

		public void Reset() {
			_windowSequenceProcess = null;
			_playTextProcess = null;
			_waitInputProcess = null;
			_waitSecondsProcess = null;

			_sequencerIndex = 0;
			_windowSequenceIndex = 0;
			_playTextIndex = 0;
			_waitInputIndex = 0;
			_waitSecondsIndex = 0;

			_waitSecondsElapsed = 0f;

			_flagStart = false;
			_flagEnd = false;
		}

		private void End() {
			_flagEnd = true;
		}

		public void Update(EntityCommandBuffer commandBuffer, AdvScript script, DynamicBuffer<InputReleasedEvent> releaseEvents, in InputMainStick mainStick) {
			if (_processor != null && script != null) {
				if (FlagStart) {
					if (_windowSequenceProcess != null) {
						_processor.ProcessWindowSequence(_windowSequenceProcess.CalledOnce, () => SetFlagWindowSequence(script, commandBuffer));
					} else if (_playTextProcess != null) {
						_processor.ProcessPlayText(
							mainStick,
							releaseEvents,
							_playTextProcess.Speaker,
							_playTextProcess.Text,
							() => SetFlagPlayerText(script, commandBuffer)
						);
					} else if (_waitInputProcess != null) {
						_processor.ProcessWaitInput(
							mainStick,
							releaseEvents,
							() => SetFlagWaitInput(script, commandBuffer)
						);
					} else if (_waitSecondsProcess != null) {
						float dt = Time.deltaTime;
						_waitSecondsElapsed += dt;
						if (_waitSecondsElapsed >= _waitSecondsProcess.Seconds) {
							SetFlagWaitSeconds(script, commandBuffer);
						}
					}
				} else {
					_flagStart = true;
					UpdateSequenceIndex(script, 0, commandBuffer);
				}
				
			}
		}

		private void SetFlagWindowSequence(AdvScript script, EntityCommandBuffer commandBuffer) {
			if (_windowSequenceProcess.CalledOnce) {
				UpdateSequenceIndex(script, _windowSequenceProcess.ParentIndex, commandBuffer);
			} else {
				_windowSequenceProcess.SetCalled(true);
				UpdateSequenceIndex(script, _windowSequenceProcess.StartIndex, commandBuffer);
			}
		}

		private void SetFlagPlayerText(AdvScript script, EntityCommandBuffer commandBuffer) {
			UpdateSequenceIndex(script, _playTextProcess.NextIndex, commandBuffer);
		}

		private void SetFlagWaitInput(AdvScript script, EntityCommandBuffer commandBuffer) {
			UpdateSequenceIndex(script, _waitInputProcess.NextIndex, commandBuffer);
		}

		private void SetFlagWaitSeconds(AdvScript script, EntityCommandBuffer commandBuffer) {
			UpdateSequenceIndex(script, _waitSecondsProcess.NextIndex, commandBuffer);
		}

		private void UpdateSequenceIndex(AdvScript script, int to, EntityCommandBuffer commandBuffer) {
			if (to >= 0) {
				SetProcessor(script, to, commandBuffer);
			} else {
				End();
			}
		}

		private void SetProcessor(AdvScript script, int to, EntityCommandBuffer commandBuffer) {
			_windowSequenceProcess = null;
			_playTextProcess = null;
			_waitInputProcess = null;
			_waitSecondsProcess = null;

			for (int i = 0; i < script.SequencerProcess.Length; ++i) {
				if (script.SequencerProcess[i].SequenceIndex == to) {
					if (i == _sequencerIndex) {
						script.SequencerProcess[i].ResetCurrentChild();
						_sequencerIndex++;
					}
					UpdateSequenceIndex(script, script.SequencerProcess[i].Next(), commandBuffer);
					break;
				} else if (i == _sequencerIndex) {
					break;
				}
			}

			for (int i = 0; i < script.WindowSequenceProcess.Length; ++i) {
				if (script.WindowSequenceProcess[i].SequenceIndex == to) {
					_windowSequenceProcess = script.WindowSequenceProcess[i];
					if (i == _windowSequenceIndex) {
						_windowSequenceProcess.SetCalled(false);
						_windowSequenceIndex++;
					}
					break;
				} else if (i == _windowSequenceIndex) {
					break;
				}
			}

			if (_playTextIndex < script.PlayTextProcess.Length) {
				if (script.PlayTextProcess[_playTextIndex].SequenceIndex == to) {
					_playTextProcess = script.PlayTextProcess[_playTextIndex];
					_playTextIndex++;
					return;
				}
			}

			if (_waitInputIndex < script.WaitInputProcess.Length) {
				if (script.WaitInputProcess[_waitInputIndex].SequenceIndex == to) {
					_waitInputProcess = script.WaitInputProcess[_waitInputIndex];
					_waitInputIndex++;
					return;
				}
			}

			if (_waitSecondsIndex < script.WaitSecondsProcess.Length) {
				if (script.WaitSecondsProcess[_waitSecondsIndex].SequenceIndex == to) {
					_waitSecondsProcess = script.WaitSecondsProcess[_waitSecondsIndex];
					_waitSecondsIndex++;
					_waitSecondsElapsed = 0.0f;
					return;
				}
			}

			foreach(AdvScript.EndProcessData end in script.EndProcess) {
				if (end.SequenceIndex == to) {
					_processor.ProcessEnd(commandBuffer, end.TypeId);
					UpdateSequenceIndex(script, -1, commandBuffer);
					return;
				}
			}
		}
	}
}