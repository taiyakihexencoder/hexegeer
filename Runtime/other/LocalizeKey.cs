using hexegeer.internallib;
using UnityEngine;

namespace hexegeer {
	[System.Serializable]
	public partial struct LocalizeKey : IPseudoLongEnum<LocalizeKey> {
		[SerializeField]
		private long _id;
		public long Id => _id;

		[SerializeField]
		private string _name;
		public string Name => _name;

		private LocalizeKey(long id, string name) {
			_id = id;
			_name = name;
		}
	}
}