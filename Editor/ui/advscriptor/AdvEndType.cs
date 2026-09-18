using hexegeer.internallib;
using UnityEngine;

namespace hexegeer {
	[System.Serializable]
	public partial struct AdvEndType : IPseudoEnum<AdvEndType> {
		[SerializeField]
		private int _id;
		public int Id => _id;

		[SerializeField]
		private string _name;
		public string Name => _name;

		private AdvEndType(int id, string name) {
			_id = id;
			_name = name;
		}
	}
}