namespace hexegeer {
	/// <summary>
	/// HexegeerWorldSystemにBelongsToとCollidesWithを渡す。
	/// HexegeerWorldSystemはそれをLayoutBlobTableに渡すことでinternallib内でイベントのCollisionLayerを設定する。
	/// </summary>
	public static partial class EventColliderLayer {
		public static uint BelongsTo { get; private set; }
		public static uint CollidesWith { get; private set; }

		static EventColliderLayer() {
			SetLayer();
		}

		static partial void SetLayer();
	}
}