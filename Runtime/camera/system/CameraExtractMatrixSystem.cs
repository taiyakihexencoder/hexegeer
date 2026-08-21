using hexegeer.internallib;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerCameraSystemGroup))]
	public partial class CameraExtractMatrixSystem : SystemBase {
		protected override void OnCreate() {
			base.OnCreate();
			EntityManager.CreateSingleton(
				new CameraMatrix { 
					projectMatrix = float4x4.identity, 
					viewMatrix = float4x4.identity,
				}, 
				"Camera Matrix@Hexegeer"
			);
		}

		protected override void OnUpdate() {
			Camera camera = Camera.main;
			if (camera != null) {
				SystemAPI.SetSingleton(new CameraMatrix {
					viewMatrix = camera.worldToCameraMatrix,
					projectMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false),
				});
			}
		}
	}
}