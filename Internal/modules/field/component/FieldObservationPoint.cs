using Unity.Entities;

namespace hexegeer.internallib {
	/// <summary>
	/// フィールド読み込みに使う位置を示す。
	/// このコンポーネントを持つEntityのLocalToWorldを用いて判定する。
	/// </summary>
	public struct FieldObservationPoint : IComponentData, IEnableableComponent { 
		public bool isPreload;
	}

	/// <summary>
	/// 初回シーンロード時はキャラクターを出す前にフィールドを生成しておく必要がある。
	/// この場合はロードしたフィールドの破棄を一定時間無効化し、
	/// ObservationPointはentityごと破棄する。
	/// </summary>
	public struct FieldPreload : IComponentData { }
}