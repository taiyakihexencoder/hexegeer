using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	/// <summary>
	/// キャラクターの移動指示
	/// </summary>
	public struct CharacterMove : IComponentData {
		/// <summary>
		/// x方向の定義
		/// </summary>
		public float3 xAxis;
		/// <summary>
		/// y方向の定義
		/// </summary>
		public float3 yAxis;
		/// <summary>
		/// z方向の定義
		/// </summary>
		public float3 zAxis;

		/// <summary>
		/// 移動情報
		/// </summary>
		public float3 move;
	}
}