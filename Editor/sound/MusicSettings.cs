using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/sound/music_settings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class MusicSettings : ScriptableSingleton<MusicSettings> {
		[System.Serializable]
		public class MusicInfo {
			public int id;
			public string name;
			public string address;
			public string description;
		}

		[SerializeField]
		private List<MusicInfo> _musicList;
		public List<MusicInfo> MusicList {
			get {
				if (_musicList == null) { _musicList = new List<MusicInfo>(); }
				return _musicList;
			}
		}

		public void Add() {
			int newId = 1;
			List<int> ids = _musicList.ConvertAll(_ => _.id);
			ids.Sort();
			foreach(int id in ids) {
				if (newId < id) {
					break;
				}
				newId = id+1;
			}

			_musicList.Add(
				new MusicInfo {
					id = newId,
					name = "new music",
					address = "",
					description = "",
				}
			);
			Save(true);
		}

		public void UpdateParameter(int index, in MusicInfo info) {
			_musicList[index] = info;
			Save(true);
		}

		public void MoveUp(int index) {
			MusicInfo tmp = _musicList[index];
			_musicList[index] = _musicList[index-1];
			_musicList[index-1] = tmp;
			Save(true);
		}

		public void MoveDown(int index) {
			MusicInfo tmp = _musicList[index];
			_musicList[index] = _musicList[index+1];
			_musicList[index+1] = tmp;
			Save(true);
		}

		public void RemoveAt(int index) {
			_musicList.RemoveAt(index);
			Save(true);
		}
	}
}