namespace hexegeer.editor {
	/// <summary>
	/// 常に用意する必要のあるレイヤー名
	/// </summary>
	public enum DefaultLayer {
		Terrain,
		PhysicsObject,
	}

	public class DefaultLayerCollision {
		internal static DefaultLayer[][] Value = new DefaultLayer[][] {
			new DefaultLayer[] { DefaultLayer.Terrain, DefaultLayer.PhysicsObject, },
			new DefaultLayer[] { DefaultLayer.Terrain, },
		};
	}
}