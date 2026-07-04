using Unity.Entities;

namespace hexegeer.internallib {
	/// <summary>
	/// 設定値のシングルトン
	/// </summary>
	public struct FieldSetting : IComponentData {
		/// <summary>
		/// 領域のふちからこの距離を読込距離とする
		/// </summary>
		public float loadFieldDistance;

		/// <summary>
		/// 領域のふちからこの距離離れたら破棄する
		/// </summary>
		public float unloadFieldDistance;

		/// <summary>
		/// フィールドメッシュのキャッシュ数
		/// キャッシュ数を超過すると古いロードデータから削除される
		/// </summary>
		public int cacheFieldMeshCount;

		/// <summary>
		/// 一回更新後に改めて更新するまでのインターバル。
		/// </summary>
		public double updateInterval;

		/// <summary>
		/// Colliderのレイヤー
		/// </summary>
		public uint belongsTo;

		/// <summary>
		/// Colliderの衝突レイヤー
		/// </summary>
		public uint collidesWith;
	}
}