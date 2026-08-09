using Unity.Entities;

namespace hexegeer {
	public partial struct UserSaveParameter : IComponentData {
		// 内容は別ファイルに自動生成される

		partial void SetDefault();

		public static UserSaveParameter defaultValue {
			get {
				UserSaveParameter parameter = new UserSaveParameter();
				parameter.SetDefault();
				return parameter;
			}
		}
	}
}