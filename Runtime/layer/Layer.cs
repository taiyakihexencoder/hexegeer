namespace hexegeer {
	public partial class Layer {
		public const uint Terrain = 1u;
		public const uint PhysicsObject = 2u;
	}

	public partial class LayerCollide {
		public const uint Terrain = Layer.Terrain | Layer.PhysicsObject;
		public const uint PhysicsObject = Layer.Terrain;
	}
}