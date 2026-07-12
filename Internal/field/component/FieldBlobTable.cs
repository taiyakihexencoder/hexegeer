using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	/// <summary>
	/// FieldTableをSystemで扱うためにComponentとして用意しSingleton運用
	/// </summary>
	public struct FieldBlobTable : IComponentData {
		public BlobAssetReference<FieldBlobAsset> asset;
	}

	public struct FieldBlobAsset {
		public BlobArray<FieldInfo> rows;
	}

	public struct FieldInfo {
		public int id;
		public int contentKey;
		public FixedString64Bytes address;
		public FixedString64Bytes name;
		public FixedString64Bytes guid;
		public float3 position;
		public quaternion rotation;
		public float3 boundsMin;
		public float3 boundsMax;
	}
}