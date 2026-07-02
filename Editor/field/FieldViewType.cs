namespace hexegeer.editor {
	public enum FieldViewType {
		SideView,
	}

	public static class FieldViewTypeExtension {
		public static System.Type GetResourceType(this FieldViewType type) {
			if (type == FieldViewType.SideView) {
				return typeof(SideViewFieldBlueprint);
			} else {
				return null;
			}
		}
	}
}