using Unity.Entities;

namespace hexegeer.internallib {
	/// <summary>
	/// フィールドの読込依頼
	/// </summary>
	public struct FieldLoadRequest : IComponentData {
		/// <summary>
		/// 読み込むグループのID
		/// </summary>
		public int id;
	}
}