using System.Collections.Generic;
using hexegeer.internallib;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Save/SaveSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class SaveSettings : ScriptableSingleton<SaveSettings> {
		[System.Serializable]
		public class GlobalSaveData {
			public List<SaveParameter> parameters;
		}

		[System.Serializable]
		public class UserSaveData {
			public List<SaveParameter> parameters;
		}

		[System.Serializable]
		public struct SaveParameter {
			public SaveParameterType type;
			public string name;
			public string defaultValue;
			public Version version;
		}

		[System.Serializable]
		public struct Progress {
			public int flagIndex;
			public string key;
			public byte value;
			public Version version;
		}

		public enum SaveParameterType {
			Int,
			Long,
			Boolean,
			String,
			Float,
			Vector2,
			Vector3,
			Color,
		}

		[SerializeField]
		private GlobalSaveData _global;
		public GlobalSaveData Global {
			get {
				if (_global == null) { _global = new GlobalSaveData(); }
				return _global;
			}
		}

		[SerializeField]
		private UserSaveData _user;
		public UserSaveData User {
			get {
				if (_user == null) { _user = new UserSaveData(); }
				return _user;
			}
		}

		[SerializeField]
		private List<Progress> _progressFlags;
		public List<Progress> ProgressFlags => _progressFlags;

		public void UpdateGlobalParameter(int index, SaveParameter parameter) {
			_global.parameters[index] = parameter;
			Save(true);
		}

		public void UpdateUserParameter(int index, SaveParameter parameter) {
			_user.parameters[index] = parameter;
			Save(true);
		}

		public void AddGlobalParameter() {
			_global.parameters.Add(
				new SaveParameter {
					name = "new parameter",
					type = SaveParameterType.Int,
					defaultValue = "0",
				}
			);
			Save(true);
		}

		public void AddUserParameter() {
			_user.parameters.Add(
				new SaveParameter {
					name = "new parameter",
					type = SaveParameterType.Int,
					defaultValue = "0",
				}
			);
			Save(true);
		}

		public void RemoveGlobalParameter(int index) {
			_global.parameters.RemoveAt(index);
			Save(true);
		}

		public void RemoveUserParameter(int index) {
			_user.parameters.RemoveAt(index);
			Save(true);
		}

		public void MoveUpGlobal(int index) {
			SaveParameter parameter = _global.parameters[index];
			_global.parameters.RemoveAt(index);
			_global.parameters.Insert(index-1, parameter);
			Save(true);
		}

		public void MoveDownGlobal(int index) {
			SaveParameter parameter = _global.parameters[index];
			_global.parameters.RemoveAt(index);
			_global.parameters.Insert(index+1, parameter);
			Save(true);
		}

		public void MoveUpUser(int index) {
			SaveParameter parameter = _user.parameters[index];
			_user.parameters.RemoveAt(index);
			_user.parameters.Insert(index-1, parameter);
			Save(true);
		}

		public void MoveDownUser(int index) {
			SaveParameter parameter = _user.parameters[index];
			_user.parameters.RemoveAt(index);
			_user.parameters.Insert(index+1, parameter);
			Save(true);
		}

		public void AddProgressFlag(string key) {
			int newId = 0;
			List<int> ids = _progressFlags.ConvertAll(_ => _.flagIndex);
			ids.Sort();

			foreach(int id in ids) {
				if (newId < id) {
					break;
				}
				newId++;
			}

			_progressFlags.Add(
				new Progress {
					flagIndex = newId,
					key = key,
					value = 0,
					version = new Version(),
				}
			);
			Save(true);
		}

		public void RemoveProgressFlag(int index) {
			_progressFlags.RemoveAt(index);
			Save(true);
		}

		public void UpdateProgressFlagKey(int index, string key) {
			UpdateProgressFlag(index, key, _progressFlags[index].value, _progressFlags[index].version);
		}

		public void UpdateProgressFlagValue(int index, byte value) {
			UpdateProgressFlag(index, _progressFlags[index].key, value, _progressFlags[index].version);
		}

		public void UpdateProgressFlagVersion(int index, Version version) {
			UpdateProgressFlag(index, _progressFlags[index].key, _progressFlags[index].value, version);
		}

		private void UpdateProgressFlag(int index, string key, byte value, Version version) {
			_progressFlags[index] = new Progress {
				flagIndex = _progressFlags[index].flagIndex,
				key = key,
				value = value,
				version = version,
			};
			Save(true);
		}

		public void MoveUpProgressFlag(int index) {
			Progress tmp = _progressFlags[index];
			_progressFlags[index] = _progressFlags[index-1];
			_progressFlags[index-1] = tmp;
			Save(true);
		}

		public void MoveDownProgressFlag(int index) {
			Progress tmp = _progressFlags[index];
			_progressFlags[index] = _progressFlags[index+1];
			_progressFlags[index+1] = tmp;
			Save(true);
		}
	}
}