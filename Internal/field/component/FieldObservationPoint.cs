using Unity.Entities;

namespace hexegeer.internallib {
	/// <summary>
	/// フィールド読み込みに使う位置を示す。
	/// このコンポーネントを持つEntityのLocalToWorldを用いて判定する。
	/// </summary>
	public struct FieldObservationPoint : IComponentData, IEnableableComponent { }
}