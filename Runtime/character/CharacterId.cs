using hexegeer.internallib;
using UnityEngine;

namespace hexegeer {
	[System.Serializable]
	public partial struct CharacterId : IPseudoEnum<CharacterId> {
		[SerializeField]
		private int _id;
		public int Id => _id;

		[SerializeField]
		private string _name;
		public string Name => _name;

		private CharacterId(int id, string name) {
			_id = id;
			_name = name;
		}
	}
}