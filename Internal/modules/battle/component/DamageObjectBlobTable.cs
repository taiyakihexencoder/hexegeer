using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct DamageObjectBlobTable : IComponentData {
		public BlobAssetReference<DamageObjectBlobAsset> damageObject;
		public BlobAssetReference<DamageObjectKeyListBlobAsset> keyTable;
	}

	// Damage Object Blob Asset

	public struct DamageObjectBlobAsset {
		public BlobArray<DamageObjectInfo> objectList;
		public BlobArray<DamageObjectColliderInfo> colliderList;
	}

	public struct DamageObjectInfo {
		public int id;
		public FixedString64Bytes name;
		public int collider;
	}

	public struct DamageObjectColliderInfo {
		public int id;
		public int belongsTo;
		public int collidesWith;
		public HitAreaShape shape;
		public float3 extent;
		public quaternion rotation;
	}

	// Damage Object Key List Blob Asset

	public struct DamageObjectKeyListBlobAsset {
		public BlobArray<DamageObjectKeyList> list;
	}

	public struct DamageObjectKeyList {
		public int key;
		public BlobArray<DamageObjectLoadElement> elements;
	}

	public struct DamageObjectLoadElement {
		public int index;
	}
}