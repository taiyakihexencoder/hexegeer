using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Version/VersionSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class VersionSettings : ScriptableSingleton<VersionSettings> {
		[System.Serializable]
		public class VersionInfo {
			public Version version;
			public string description;
		}

		[SerializeField]
		private List<VersionInfo> _versions;
		public List<VersionInfo> Versions => _versions;

		public void UpdateVersion(int index, in VersionInfo version) {
			_versions[index] = version;
			Save(true);
		}

		/// <summary>
		/// Minorバージョンの末尾patchのみ削除する想定。
		/// Minorバージョンが1個しかなければ以降のバージョンが繰り下がる。
		/// </summary>
		public void RemoveVersion(int index) {
			VersionInfo v = _versions[index];
			VersionInfo prev = index == 0 ? null : _versions[index-1];

			if ((prev == null || prev.version.major != v.version.major || prev.version.minor != v.version.minor) && index < _versions.Count-1) {
				VersionInfo next = _versions[index+1];
				if (next.version.major != v.version.major) {
					// 次を見てMajorバージョンが異なる場合、自身のMinorバージョンが0なら、
					// Majorバージョンが消えるので以降のMajorバージョンをすべて-1
					if (v.version.minor == 0) {
						for (int i = index+1; i < _versions.Count; ++i) {
							_versions[i].version.major -= 1;
						}
					}
				} else {
					// 次を見てMinorバージョンが異なる場合は
					// Minorバージョンが消えるのでMajorバージョンが同じもののMinorバージョンをすべて-1
					for (int i = index+1; i < _versions.Count; ++i) {
						if (v.version.major != _versions[i].version.major) { break;}
						_versions[i].version.minor -= 1;
					}
				}
			}
			_versions.RemoveAt(index);
			Save(true);
		}

		public void AddMajorVersion(VersionInfo from) {
			_versions.Add(
				new VersionInfo {
					version = new Version {
						major = from == null ? 0 : from.version.major+1,
						minor = from == null ? 1 : 0,
						patch = 0,
					},
					description = "",
				}
			);
			_versions.Sort(Sort);
			Save(true);
		}

		public void AddMinorVersion(VersionInfo from) {
			_versions.Add(
				new VersionInfo {
					version = new Version {
						major = from.version.major,
						minor = from.version.minor+1,
						patch = 0,
					},
					description = "",
				}
			);
			_versions.Sort(Sort);
			Save(true);
		}

		public void AddPatchVersion(VersionInfo from) {
			_versions.Add(
				new VersionInfo {
					version = new Version {
						major = from.version.major,
						minor = from.version.minor,
						patch = from.version.patch+1,
					},
					description = "",
				}
			);
			_versions.Sort(Sort);
			Save(true);
		}

		private int Sort(VersionInfo a, VersionInfo b) {
			int major = a.version.major.CompareTo(b.version.major);
			if (major != 0) return major;
			int minor = a.version.minor.CompareTo(b.version.minor);
			if (minor != 0) return minor;
			return a.version.patch.CompareTo(b.version.patch);
		}

		public ListPopupBuilder<Version> GetPopupBuilder() {
			ListPopupBuilder<Version> builder = new ListPopupBuilder<Version>();
			List<Version> versions = _versions.ConvertAll(_ => _.version);
			versions.Insert(0, new Version{major = 0, minor = 0, patch = 0});
			builder.SetKeys(_versions.ConvertAll(_ => _.version));
			builder.SetConverter(_ => $"{_.major}.{_.minor}.{_.patch}");
			return builder;
		}
	}
}