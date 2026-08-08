namespace hexegeer.internallib {
	[System.Serializable]
	public struct Version {
		public int major;
		public int minor;
		public int patch;

		public override bool Equals(object obj){
			return obj is Version v &&
				major == v.major &&
				minor == v.minor &&
				patch == v.patch;
		}

		public override int GetHashCode() {
			return System.HashCode.Combine(major, minor, patch);
		}
	}
}