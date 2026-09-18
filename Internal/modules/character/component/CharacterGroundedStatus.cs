using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct CharacterGroundedStatus : IComponentData {
		/// <summary>
		/// 接触判定で接地している
		/// </summary>
		public bool physicallyGrounded;

		/// <summary>
		/// 地面の傾き
		/// </summary>
		public float3 normal;

		/// <summary>
		/// 床として判定する角度のcos値
		/// </summary>
		public float groundThreshold;
		
		/// <summary>
		/// 地面に吸着
		/// </summary>
		public bool snapToGround;

		/// <summary>
		/// 吸着処理を無視
		/// </summary>
		public bool ignoreSnapToGround;

		/// <summary>
		/// 吸着処理が有効な場合、吸着先の座標
		/// </summary>
		public float3 translate;
	}
}