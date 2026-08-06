using System.Collections.Generic;
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
	}
}