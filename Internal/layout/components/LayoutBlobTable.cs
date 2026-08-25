using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer.internallib {
	public struct LayoutBlobTable : IComponentData {
		public EventCollideInfo eventCollideInfo;
		public BlobAssetReference<LayoutBlobAsset> asset;
	}

	public struct LayoutBlobAsset {
		public BlobArray<LayoutProfile> rows;
	}

	public struct LayoutProfile {
		public int contentKey;

		public BlobArray<LayoutLoadCharacterInfo> loadCharacters;
		public BlobArray<LayoutCharacterInfo> characterLayout;

		public BlobArray<LayoutEventInfo> eventLayout;
	}

	public struct LayoutLoadCharacterInfo {
		public int id;
	}

	public struct LayoutCharacterInfo {
		public int id;
		public float3 position;
		public quaternion rotation;
	}

	public struct EventCollideInfo {
		public uint belongsTo;
		public uint collidesWith;
	}

	public struct LayoutEventInfo {
		public int eventId;
		public float3 position;
		public quaternion rotation;
		public HitAreaShape shape;
		public float3 extent;
	}
}