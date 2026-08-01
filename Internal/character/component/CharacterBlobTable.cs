
using Unity.Collections;
using Unity.Entities;

public struct CharacterBlobTable : IComponentData {
	public int physicsObjectLayer;
	public int physicsObjectCollides;

	public BlobAssetReference<CharacterBlobAsset> character;
	public BlobAssetReference<CharacterColliderBlobAsset> collider;
	public BlobAssetReference<CharacterLoadTableBlobAsset> loadTable;
}

public struct CharacterBlobAsset {
	public BlobArray<CharacterInfo> rows;
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
	public FixedString128Bytes modelAsset;
	public int collider;
	public int belongsTo;
	public int collidesWith;
	public bool hasObservationPoint;
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
