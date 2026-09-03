using hexegeer.internallib;
using UnityEngine;

namespace hexegeer {
	[System.Serializable]
	public partial struct MasterDataKey : IPseudoEnum<MasterDataKey> {
		[SerializeField]
		private int _id;
		public int Id => _id;

		[SerializeField]
		private string _name;
		public string Name => _name;

		[SerializeField]
		private string _fileName;
		public string FileName => _fileName;

		private MasterDataKey(int id, string name, string fileName) {
			_id = id;
			_name = name;
			_fileName = fileName;
		}
	}
}