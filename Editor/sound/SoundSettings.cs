using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/sound/sound_settings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class SoundSettings : ScriptableSingleton<SoundSettings> {
		[System.Serializable]
		public class SoundInfo {
			public int id;
			public string name;
			public string address;
		}

		[SerializeField]
		private List<SoundInfo> _systemSoundList;
		public List<SoundInfo> SystemSoundList {
			get {
				if (_systemSoundList == null) { _systemSoundList = new List<SoundInfo>(); }
				return _systemSoundList;
			}
		}

		[SerializeField]
		private List<SoundInfo> _environmentSoundList;
		public List<SoundInfo> EnvironmentSoundList {
			get {
				if (_environmentSoundList == null) { _environmentSoundList = new List<SoundInfo>(); }
				return _environmentSoundList;
			}
		}

		public void AddSystemSound() {
			int newId = 1;
			List<int> ids = SystemSoundList.ConvertAll(_ => _.id);
			ids.Sort();
			foreach(int id in ids) {
				if (newId < id) { break; }
				newId = id+1;
			}

			SystemSoundList.Add(
				new SoundInfo {
					id = newId,
					name = "new sound",
					address = "",
				}
			);
			Save(true);
		}

		public void AddEnvironmentSound() {
			int newId = 1;
			List<int> ids = EnvironmentSoundList.ConvertAll(_ => _.id);
			ids.Sort();
			foreach(int id in ids) {
				if (newId < id) { break; }
				newId = id+1;
			}

			EnvironmentSoundList.Add(
				new SoundInfo {
					id = newId,
					name = "new sound",
					address = "",
				}
			);
			Save(true);
		}

		public void UpdateSystemSound(int index, in SoundInfo info) {
			_systemSoundList[index] = info;
			Save(true);
		}

		public void MoveUpSystemSound(int index) {
			SoundInfo tmp = _systemSoundList[index];
			_systemSoundList[index] = _systemSoundList[index-1];
			_systemSoundList[index-1] = tmp;
			Save(true);
		}

		public void MoveDownSystemSound(int index) {
			SoundInfo tmp = _systemSoundList[index];
			_systemSoundList[index] = _systemSoundList[index+1];
			_systemSoundList[index+1] = tmp;
			Save(true);
		}

		public void RemoveSystemSound(int index) {
			_systemSoundList.RemoveAt(index);
			Save(true);
		}

		public void UpdateEnvironmentSound(int index, in SoundInfo info) {
			_environmentSoundList[index] = info;
			Save(true);
		}

		public void MoveUpEnvironmentSound(int index) {
			SoundInfo tmp = _environmentSoundList[index];
			_environmentSoundList[index] = _environmentSoundList[index-1];
			_environmentSoundList[index-1] = tmp;
			Save(true);
		}

		public void MoveDownEnvironmentSound(int index) {
			SoundInfo tmp = _environmentSoundList[index];
			_environmentSoundList[index] = _environmentSoundList[index+1];
			_environmentSoundList[index+1] = tmp;
			Save(true);
		}

		public void RemoveEnvironmentSound(int index) {
			_environmentSoundList.RemoveAt(index);
			Save(true);
		}
	}
}