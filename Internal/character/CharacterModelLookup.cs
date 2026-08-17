using System.Collections.Generic;

namespace hexegeer.internallib {
	public static class CharacterModelLookup {
		private static readonly object _lockHandle = new object();
		private static Dictionary<int, CharacterTable.ModelProfile> _table;

		static CharacterModelLookup() {
			_table = new Dictionary<int, CharacterTable.ModelProfile>();
		}

		public static void Register(in CharacterTable.Character character) {
			lock (_lockHandle) {
				if (!_table.ContainsKey(character.id)) {
					_table.Add(
						character.id, 
						new CharacterTable.ModelProfile {
							modelAsset = character.modelProfile.modelAsset,
							overrideAnimations = new List<string>(character.modelProfile.overrideAnimations),
							additiveAnimations = new List<string>(character.modelProfile.additiveAnimations),
							baseAnimations = new List<string>(character.modelProfile.baseAnimations),
						}
					);
				}
			}
		}

		public static bool TryGetProfile(int id, out CharacterTable.ModelProfile profile) {
			return _table.TryGetValue(id, out profile);
		}
	}
}