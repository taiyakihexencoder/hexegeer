namespace hexegeer {
	public readonly partial struct ContentKey {
		public readonly int value;

		public ContentKey(int key) {
			this.value = key;
		}

		public override bool Equals(object obj) {
			return obj is ContentKey contentKey && contentKey.value == value;
		}

		public override int GetHashCode() {
			return value.GetHashCode();
		}
	}
}