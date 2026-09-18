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

		/// <summary>
		/// 一定時間アンロードを無視する
		/// </summary>
		public bool keep;
	}
}