using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	/// <summary>
	/// フィールドの基本情報
	/// </summary>
	public struct FieldHeader : IComponentData {
		/// <summary>
		/// 識別子
		/// </summary>
		public int id;

		/// <summary>
		/// コンテンツキー
		/// </summary>
		public int contentKey;

		/// <summary>
		/// LunarscapeFieldMeshComponentの読込状態
		/// 読込開始でONになる。
		/// </summary>
		public bool active;

		/// <summary>
		/// 最後に更新した時間。
		/// 無闇に更新しないようにインターバルを設ける。
		/// </summary>
		public double lastUpdated;

		/// <summary>
		/// フィールドの範囲
		/// </summary>
		public float3 boundsMin;

		/// <summary>
		/// フィールドの範囲
		/// </summary>
		public float3 boundsMax;
	}
}