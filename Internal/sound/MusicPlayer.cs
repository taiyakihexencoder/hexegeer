using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace hexegeer.internallib {
	public class MusicPlayer {
		private class CachedMusic {
			public int id;
			public string address;
			public AudioClip clip;
		}

		private const string RES_AUDIO_MIXER = "AudioMixer";
		private const string MIXER_GROUP = "Music";
		private const string RES_AUDIO_SOURCE = "AudioSource";

		private const int CLIP_CACHE_SIZE = 10;
		private const int SOURCE_COUNT = 3;
		private const float FADE_SECONDS = 0.5f;

		private AudioMixer _mixer;
		private AudioMixerGroup _mixerGroup;
		private AudioSource[] _audioSources;

		private MusicTable _musicTable;

		private List<CachedMusic> _musics;

		private int _currentActiveIndex;

		CancellationTokenSource _cancelTaskSource;

		public bool LoadRequested{ get; private set; } = false;
		public bool Ready => _musicTable != null;

		public MusicPlayer() {
			_mixer = Resources.Load<AudioMixer>(RES_AUDIO_MIXER);
			_mixerGroup = _mixer.FindMatchingGroups(MIXER_GROUP)[0];
			_musicTable = null;
			_musics = new List<CachedMusic>();
			_currentActiveIndex = 0;
			_cancelTaskSource = null;

			AudioSource prefab = Resources.Load<AudioSource>(RES_AUDIO_SOURCE);
			_audioSources = new AudioSource[SOURCE_COUNT];
			for (int i = 0; i < _audioSources.Length; ++i) {
				_audioSources[i] = Object.Instantiate(prefab);
				_audioSources[i].name = $"BGM-{i+1}";
				_audioSources[i].outputAudioMixerGroup = _mixerGroup;
				Object.DontDestroyOnLoad(_audioSources[i].gameObject);
			}
		}

		public void Dispose() {
			_cancelTaskSource?.Cancel();
		}

		private async Task UpdateProcess(CancellationToken token) {
			float elapsed = 0.0f;

			while (elapsed < FADE_SECONDS) {

				if (token.IsCancellationRequested) {
					return;
				}

				SyncContext.Send ( () => {
					float dt = Time.deltaTime;
					elapsed += dt;
					Update(dt);
				});
				await Task.Yield();
			}
			SyncContext.Post( () => {
				for(int i = 0; i < SOURCE_COUNT; ++i) {
					_audioSources[i].volume = i == _currentActiveIndex ? 1f : 0f;
				}
			});
		}

		private void Update(float dt) {
			float delta = dt / FADE_SECONDS;
			for (int i = 0; i < SOURCE_COUNT; ++i) {
				if (i == _currentActiveIndex) {
					_audioSources[i].volume += delta;
					if (_audioSources[i].volume > 1.0f) { _audioSources[i].volume = 1.0f; }
				} else {
					_audioSources[i].volume -= delta;
					if (_audioSources[i].volume < 0.0f) { 
						_audioSources[i].volume = 0.0f; 
						_audioSources[i].Stop();
						_audioSources[i].clip = null;
					}
				}
			}
		}

		public void RequestPlay(int id) {
			Task.Run(async () => {
				if (_musicTable == null) {
					if (!LoadRequested) {
						LoadRequested = true;
						_musicTable = await AssetUtil.RequestLoad<MusicTable>(MusicTable.RESOURCE_ADDRESS);
					}

					_cancelTaskSource = new CancellationTokenSource();
					CancellationToken token = _cancelTaskSource.Token;
					while (_musicTable == null) {
						if (token.IsCancellationRequested) {
							return;
						}
						await Task.Yield();
					}
				}

				SyncContext.Post(() => {
					Play(id);
				});
			});
		}

		private void Play(int id) {
			AudioClip clip = null;
			for (int i = 0; i < _musics.Count; ++i) {
				if (_musics[i].id == id) {
					clip = _musics[i].clip;
					_musics.Add(_musics[i]);
					_musics.RemoveAt(i);
					break;
				}
			}

			if (clip != null) {
				StartMusic(clip);
			} else {
				foreach(MusicTable.MusicInfo row in _musicTable.Rows) {
					if (row.id == id) {
						LoadMusic(id, row.address);
					}
				}
			}
		}

		public void LoadMusic(int id, string address) {
			Task.Run(async () => {
				AudioClip clip = await AssetUtil.RequestLoad<AudioClip>(address);
				_musics.Add(
					new CachedMusic {
						id = id,
						address = address,
						clip = clip,
					}
				);

				SyncContext.Post(() => {
					StartMusic(clip);
				});

				while (_musics.Count > CLIP_CACHE_SIZE) {
					AssetUtil.Release(_musics[0].address);
					_musics.RemoveAt(0);
				}
			});
		}

		private void StartMusic(AudioClip clip) {
			_currentActiveIndex = (_currentActiveIndex + 1) % SOURCE_COUNT;

			AudioSource audioSource = _audioSources[_currentActiveIndex];
			if (audioSource.isPlaying) {
				audioSource.Stop();
			}
			audioSource.clip = clip;
			audioSource.volume = 0f;
			audioSource.Play();

			if (_cancelTaskSource != null) {
				_cancelTaskSource.Cancel();
				_cancelTaskSource.Dispose();
			}

			_cancelTaskSource = new CancellationTokenSource();

			Task.Run(
				async () => { await UpdateProcess(_cancelTaskSource.Token); }, 
				_cancelTaskSource.Token
			);
		}

		public void OnDestroy() {
			_cancelTaskSource?.Cancel();
			_cancelTaskSource = null;

			for (int i = 0; i < _musics.Count; ++i) {
				AssetUtil.Release(_musics[i].address);
			}
			_musics.Clear();

			if (_musicTable != null) {
				_musicTable = null;
				AssetUtil.Release(MusicTable.RESOURCE_ADDRESS);
			}
		}
	}
}