using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public struct CharacterPhysical : IComponentData {
		/// <summary>
		/// 接地している
		/// </summary>
		public bool isGrounded;

		/// <summary>
		/// 地面の傾き
		/// </summary>
		public float3 normal;

		/// <summary>
		/// キャラクターの向き
		/// </summary>
		public quaternion rotation;
	}
}