using Unity.Entities;

namespace hexegeer.internallib {
	public enum HexegeerSystemModule {
		Event,
	}

	public struct HexegeerStartSystemModuleRequest : IComponentData {
		public HexegeerSystemModule module;
	}

	public struct HexegeerEndSystemModuleRequest : IComponentData {
		public HexegeerSystemModule module;
	}

	public struct HexegeerSystemModuleEventComponent : IComponentData { }
}