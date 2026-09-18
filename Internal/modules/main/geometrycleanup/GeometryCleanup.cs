using Unity.Entities;
using Unity.Physics;

namespace hexegeer.internallib {
	public struct GeometryCleanup : IComponentData, ICleanupComponentData {
		public BlobAssetReference<Collider> geometry; 
	}
}