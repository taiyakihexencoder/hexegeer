using UnityEngine;

namespace hexegeer.internallib {
	public sealed class FieldMeshResource : ScriptableObject {
		/// <summary>
		/// エディタアセットのGuidを識別子として使用
		/// </summary>
		[SerializeField]
		private string _guid;
		public string Guid => _guid;

		[SerializeField]
		private string[] _subassets;
		public string[] Subassets => _subassets;
	}
}