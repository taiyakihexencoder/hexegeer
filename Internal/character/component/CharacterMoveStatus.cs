using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct CharacterMoveStatus : IComponentData {
		/// <summary>
		/// 受けている外力
		/// </summary>
		public float3 force;

		/// <summary>
		/// スピードの変動
		/// </summary>
		public float3 velocityChanges;

		/// <summary>
		/// 向く方向
		/// </summary>
		public quaternion lookDirection;

		/// <summary>
		/// キャラクターの向きを変更する移動量の閾値
		/// </summary>
		public float lookDirectionThreshold;

		/// <summary>
		/// 移動リクエストと速度を一致させるためにかかる秒数
		/// </summary>
		public float correctionSeconds;

	}
}