using UnityEngine;
using UnityEngine.Audio;

namespace hexegeer.internallib {
	public sealed class SoundPlayer {
		private AudioMixer _mixer;

		private const string RES_AUDIO_MIXER = "AudioMixer";
		private const string MIXER_SYSTEM_GROUP = "SystemSoundEffect";
		private const string MIXER_ENV_GROUP = "SoundEffect";
		private const string RES_AUDIO_SOURCE = "AudioSource";

		private EnvironmentSoundTable _environmentSoundTable;
		private SystemSoundTable _systemSoundTable;

		private AudioSource _systemSound;
		private AudioSource _envSound;

		public SoundPlayer(EnvironmentSoundTable environmentSoundtable, SystemSoundTable systemSoundTable) {
			_environmentSoundTable = environmentSoundtable;
			_systemSoundTable = systemSoundTable;

			_mixer = Resources.Load<AudioMixer>(RES_AUDIO_MIXER);
			AudioMixerGroup systemGroup = _mixer.FindMatchingGroups(MIXER_SYSTEM_GROUP)[0];
			AudioMixerGroup envGroup = _mixer.FindMatchingGroups(MIXER_ENV_GROUP)[0];

			AudioSource prefab = Resources.Load<AudioSource>(RES_AUDIO_SOURCE);

			_systemSound = Object.Instantiate(prefab);
			_systemSound.name = "System Sound";
			_systemSound.outputAudioMixerGroup = systemGroup;
			Object.DontDestroyOnLoad(_systemSound.gameObject);

			_envSound = Object.Instantiate(prefab);
			_envSound.name = "Environment Sound";
			_envSound.outputAudioMixerGroup = envGroup;
			Object.DontDestroyOnLoad(_envSound.gameObject);
		}

		private void PlaySystemSound(AudioClip clip) {
			_systemSound.PlayOneShot(clip);
		}

		private void PlayEnvironmentSound(AudioClip clip) {
			_envSound.PlayOneShot(clip);
		}

		public void PlaySystemSound(int id) {

		}

		public void PlayEnvironmentSound(int id) {

		}
	}
}