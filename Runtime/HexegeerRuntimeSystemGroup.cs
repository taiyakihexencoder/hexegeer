using hexegeer.internallib;
using Unity.Entities;

namespace hexegeer {
	/// <summary>
	/// プロジェクトで配置する用のGroup。
	/// HexegeerWorldInstanceが生成されている状態で有効になる
	/// </summary>
	[UpdateInGroup(typeof(HexegeerWorldSystemGroup))]
	public partial class HexegeerWorldUpdateGroup : ComponentSystemGroup { }

	/// <summary>
	/// ライブラリの物理処理の前に実行する
	/// </summary>
	[UpdateInGroup(typeof(HexegeerBeforePhysicsSystemGroup), OrderFirst = true)]
	public partial class HexegeerBeforePhysicsProcessGroup : ComponentSystemGroup { }
}