using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct DamageObjectBlobTable : IComponentData {
		public DamageObjectBlobAsset damageObject;
		public DamageObjectKeyListBlobAsset keyTable;
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
		public int belongsTo;
		public int collidesWith;
	}

	public struct DamageObjectColliderInfo {
		public int id;
		public HitAreaShape shape;
		public float3 extent;
	}

	// Damage Object Key List Blob Asset

	public struct DamageObjectKeyListBlobAsset {
		public int key;
		public BlobArray<DamageObjectLoadElement> list;
	}

	public struct DamageObjectLoadElement {
		public int index;
	}
}