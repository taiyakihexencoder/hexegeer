using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public struct CameraMatrix : IComponentData {
		public float4x4 viewMatrix;
		public float4x4 projectMatrix;

		/// <summary>
		/// 画面内であれば、x,y座標は0-1の座標、z座標は0より大きい。
		/// それ以外の場合はカメラ範囲に存在しない。
		/// z座標が負の場合はカメラの裏側。
		/// </summary>
		public readonly float3 GetScreenPos(in float3 worldPosition) {
			float4x4 vpMat = math.mul(projectMatrix, viewMatrix);

			float4 clipSpace = math.mul(vpMat, new float4(worldPosition, 1.0f));

			if (clipSpace.w == 0.0f) {
				return new float3(0f,0f,0f);
			} else {
				return new float3(
					(clipSpace.x / clipSpace.w + 1f) * 0.5f,
					(clipSpace.y / clipSpace.w + 1f) * 0.5f,
					clipSpace.w
				);
			}
		}
	}
}