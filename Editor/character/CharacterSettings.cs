using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Character/CharacterSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class CharacterSettings : ScriptableSingleton<CharacterSettings> {
		[System.Serializable]
		public class CharacterData {
			public int id;
			public int layer;
			public int collider;
			public int[] contentKeys;
			public string name;

			public List<HitArea> hitAreas;

			public ModelProfile modelProfile;
		}

		[System.Serializable]
		public class HitArea {
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 extent;
			public HitAreaShape shape;
		}

		[System.Serializable]
		public class ModelProfile {
			public string modelAsset;
			public List<string> overrideAnimations;
			public List<string> additiveAnimations;
			public List<string> baseAnimations;
		}

		[SerializeField]
		private List<CharacterData> _characters;
		public List<CharacterData> Characters => _characters;

		[SerializeField]
		private int[] _observationPoint;
		public int[] ObservationPoint => _observationPoint;

		public void SetName(CharacterData characterData, string name) {
			UpdateItem(characterData, character => character.name = name);
		}

		public void SetModelAsset(CharacterData characterData, string modelAsset) {
			UpdateItem(characterData, character => character.modelProfile.modelAsset = modelAsset);
		}

		public void SetOverrideAnimation(CharacterData characterData, int index, string address) {
			UpdateItem(characterData, character => character.modelProfile.overrideAnimations[index] = address);
		}

		public void SetAdditiveAnimation(CharacterData characterData, int index, string address) {
			UpdateItem(characterData, character => character.modelProfile.additiveAnimations[index] = address);
		}

		public void SetBaseAnimation(CharacterData characterData, int index, string address) {
			UpdateItem(characterData, character => character.modelProfile.baseAnimations[index] = address);
		}

		public void DeleteOverrideAnimation(CharacterData characterData, int index) {
			UpdateItem(characterData, character => character.modelProfile.overrideAnimations.RemoveAt(index));
		}

		public void DeleteAdditiveAnimation(CharacterData characterData, int index) {
			UpdateItem(characterData, character => character.modelProfile.additiveAnimations.RemoveAt(index));
		}

		public void DeleteBaseAnimation(CharacterData characterData, int index) {
			UpdateItem(characterData, character => character.modelProfile.baseAnimations.RemoveAt(index));
		}

		public void AddOverrideAnimation(CharacterData characterData) {
			UpdateItem(characterData, character => character.modelProfile.overrideAnimations.Add(""));
		}

		public void AddAdditiveAnimation(CharacterData characterData) {
			UpdateItem(characterData, character => character.modelProfile.additiveAnimations.Add(""));
		}

		public void AddBaseAnimation(CharacterData characterData) {
			UpdateItem(characterData, character => character.modelProfile.baseAnimations.Add(""));
		}

		public void SetLayer(CharacterData characterData, int layer) {
			UpdateItem(characterData, character => character.layer = layer);
		}

		public void SetCollider(CharacterData characterData, int collider) {
			UpdateItem(characterData, character => character.collider = collider);
		}

		public void SetHitArea(CharacterData characterData, List<HitArea> areas) {
			UpdateItem(characterData, character => character.hitAreas = areas);
		}

		private void UpdateItem(CharacterData characterData, System.Action<CharacterData> action) {
			for (int i = 0; i < _characters.Count; ++i) {
				if (_characters[i].id == characterData.id) {
					action(_characters[i]);
					Save(true);
					break;
				}
			}
		}

		public void AddContentKeys(CharacterData characterData, int contentKey) {
			for (int i = 0; i < _characters.Count; ++i) {
				if (_characters[i].id == characterData.id) {
					for (int j = 0; j < _characters[i].contentKeys.Length; ++j) {
						if (_characters[i].contentKeys[j] == contentKey) {
							return;
						}
					}
					int[] keys = new int[_characters[i].contentKeys.Length + 1];
					System.Array.Copy(_characters[i].contentKeys, keys, _characters[i].contentKeys.Length);
					keys[_characters[i].contentKeys.Length] = contentKey;
					_characters[i].contentKeys = keys;
					Save(true);
					break;
				}
			}
		}

		public void RemoveContentKey(CharacterData characterData, int contentKey) {
			for (int i = 0; i < _characters.Count; ++i) {
				if (_characters[i].id == characterData.id) {
					for (int j = 0; j < _characters[i].contentKeys.Length; ++j) {
						if (_characters[i].contentKeys[j] == contentKey) {
							int[] keys = new int[_characters[i].contentKeys.Length-1];
							for (int k = 0; k < j; ++k) {
								keys[k] = _characters[i].contentKeys[k];
							}

							for (int k = j; k < keys.Length; ++k) {
								keys[k] = _characters[i].contentKeys[k+1];
							}
							_characters[i].contentKeys = keys;
							Save(true);
							break;
						}
					}
				}
			}
		}

		public void Add(string name) {
			int id = 1;

			List<int> ids = _characters.Map(_ => _.id);
			ids.Sort();
			for(int i = 0; i < ids.Count; ++i) {
				if (ids[i] - id > 0) { break; }
				id = ids[i] + 1;
			}

			_characters.Add( 
				new CharacterData {
					id = id,
					layer = LayerSettings.GetSelectableFirstLayer(),
					collider = 0,
					contentKeys = new int[0],
					name = name,
					modelProfile = new ModelProfile {
						modelAsset = "",
						overrideAnimations = new List<string>(),
						additiveAnimations = new List<string>(),
						baseAnimations = new List<string>(),
					}
				}
			);
			Save(true);
		}

		public void Remove(int id) {
			_characters.RemoveAll(_ => _.id == id);
			Save(true);
		}

		public ListPopupBuilder<int> CreateListPopupBuilder() {
			ListPopupBuilder<int> builder = new ListPopupBuilder<int>();

			builder.SetConverter(key => {
				foreach(CharacterData character in instance.Characters) {
					if (character.id == key) { return character.name; }
				}
				return " - ";
			});

			builder = UpdateKeys(builder);
			return builder;
		}

		public ListPopupBuilder<int> UpdateKeys(ListPopupBuilder<int> builder) {
			List<int> keys = new List<int>(){0};
			foreach(CharacterData character in Characters) {
				keys.Add(character.id);
			}

			return builder.SetKeys(keys);
		}

		public bool IsObservationPoint(CharacterData character) {
			foreach(int id in _observationPoint) {
				if (character.id == id) { return true; }
			}
			return false;
		}

		public void AddObservationPoint(int id) {
			for (int i = 0; i < _observationPoint.Length; ++i) {
				if (_observationPoint[i] == id) {
					return;
				}
			}

			int[] newList = new int[_observationPoint.Length+1];
			System.Array.Copy(_observationPoint, newList, _observationPoint.Length);
			newList[_observationPoint.Length] = id;
			_observationPoint = newList;
			Save(true);
		}

		public void RemoveObservationPoint(int id) {
			for (int i = 0; i < _observationPoint.Length; ++i) {
				if (_observationPoint[i] == id) {
					int[] newList = new int[_observationPoint.Length-1];
					System.Array.Copy(_observationPoint, newList, i);
					System.Array.Copy(_observationPoint, i+1, newList, i, newList.Length-i);
					_observationPoint = newList;
					Save(true);
					return;
				}
			}
		}
	}
}