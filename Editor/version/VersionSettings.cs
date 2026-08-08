using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Version/VersionSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class VersionSettings : ScriptableSingleton<VersionSettings> {
		[System.Serializable]
		public class Version {
			public int major;
			public int minor;
			public int patch;
			public string description;
		}

		[SerializeField]
		private List<Version> _versions;
		public List<Version> Versions => _versions;

		public void UpdateVersion(int index, in Version version) {
			_versions[index] = version;
			Save(true);
		}

		/// <summary>
		/// Minorバージョンの末尾patchのみ削除する想定。
		/// Minorバージョンが1個しかなければ以降のバージョンが繰り下がる。
		/// </summary>
		public void RemoveVersion(int index) {
			Version v = _versions[index];
			Version prev = index == 0 ? null : _versions[index-1];

			if ((prev == null || prev.major != v.major || prev.minor != v.minor) && index < _versions.Count-1) {
				Version next = _versions[index+1];
				if (next.major != v.major) {
					// 次を見てMajorバージョンが異なる場合、自身のMinorバージョンが0なら、
					// Majorバージョンが消えるので以降のMajorバージョンをすべて-1
					if (v.minor == 0) {
						for (int i = index+1; i < _versions.Count; ++i) {
							_versions[i].major -= 1;
						}
					}
				} else {
					// 次を見てMinorバージョンが異なる場合は
					// Minorバージョンが消えるのでMajorバージョンが同じもののMinorバージョンをすべて-1
					for (int i = index+1; i < _versions.Count; ++i) {
						if (v.major != _versions[i].major) { break;}
						_versions[i].minor -= 1;
					}
				}
			}
			_versions.RemoveAt(index);
			Save(true);
		}

		public void AddMajorVersion(Version from) {
			_versions.Add(
				new Version {
					major = from == null ? 0 : from.major+1,
					minor = 0,
					patch = 0,
					description = "",
				}
			);
			_versions.Sort(Sort);
			Save(true);
		}

		public void AddMinorVersion(Version from) {
			_versions.Add(
				new Version {
					major = from.major,
					minor = from.minor+1,
					patch = 0,
					description = "",
				}
			);
			_versions.Sort(Sort);
			Save(true);
		}

		public void AddPatchVersion(Version from) {
			_versions.Add(
				new Version {
					major = from.major,
					minor = from.minor,
					patch = from.patch+1,
					description = "",
				}
			);
			_versions.Sort(Sort);
			Save(true);
		}

		private int Sort(Version a, Version b) {
			int major = a.major.CompareTo(b.major);
			if (major != 0) return major;
			int minor = a.minor.CompareTo(b.minor);
			if (minor != 0) return minor;
			return a.patch.CompareTo(b.patch);
		}
	}
}