
using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct CharacterBlobTable : IComponentData {
	public int physicsObjectLayer;
	public int physicsObjectCollides;

	public BlobAssetReference<CharacterBlobAsset> character;
	public BlobAssetReference<CharacterColliderBlobAsset> collider;
	public BlobAssetReference<CharacterLoadTableBlobAsset> loadTable;
}

public struct CharacterBlobAsset {
	public BlobArray<CharacterInfo> rows;
	public BlobArray<CharacterHitAreaListAsset> hitArea;
}

public struct CharacterColliderBlobAsset {
	public BlobArray<CharacterColliderInfo> rows;
}

public struct CharacterLoadTableBlobAsset {
	public BlobArray<CharacterLoadListAsset> rows;
}

public struct CharacterLoadListAsset {
	public int key;
	public BlobArray<CharacterLoadElement> list;
}

public struct CharacterInfo {
	public int id;
	public FixedString64Bytes name;
	public int collider;
	public int belongsTo;
	public int collidesWith;
	public bool hasObservationPoint;
}

public struct CharacterHitAreaListAsset {
	public BlobArray<CharacterHitArea> list;
}

public struct CharacterHitArea {
	public HitAreaShape shape;
	public float3 extent;
	public float3 position;
	public quaternion rotation;
}

public struct CharacterColliderInfo {
	public int id;
	public FixedString64Bytes name;
	public float radius;
	public float height;
}

public struct CharacterLoadElement {
	public int index;
}
