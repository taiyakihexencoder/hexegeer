using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace hexegeer.editor {
	public sealed class AddressableListGenerator : SourceCodeGenerator {
		private class Node {
			public string address;
			public Node parent;
			public List<Node> children;
			public Node(string address) {
				parent = null;
				this.address = address;
				children = new List<Node>();
			}

			public Node Add(string address) {
				Node node = new Node(address);
				node.parent = this;
				children.Add(node);
				return node;
			}

			public Node Find(string address) {
				return children.Find(_ => _.address == address);
			}
		}

		protected override void WriteScript() {
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
			List<string[]> addressList = new List<string[]>();
			List<string> rawAddressList = new List<string>();
			List<int> indices = new List<int>();

			Regex regex = new Regex(@"[\@\-\. \(]+");
			foreach (AddressableAssetGroup group in settings.groups) {
				foreach (AddressableAssetEntry entry in group.entries) {
					string address = entry.address;
					rawAddressList.Add(address);
					indices.Add(indices.Count);
					if (!string.IsNullOrEmpty(address)) {
						if ('0' <= address[0] && address[0] <= '9') {
							address = $"_{address}";
						}
						address = regex.Replace(address, "_").Replace(")", "");
						addressList.Add(address.Split("/"));
					}
				}
			}
			indices.Sort((a, b) => {
				int length = addressList[a].Length > addressList[b].Length ? addressList[b].Length : addressList[a].Length;
				for (int i = 0; i < length; ++i) {
					int compare = addressList[a][i].CompareTo(addressList[b][i]);
					if (compare != 0) { return compare; }
				}
				return addressList[a].Length.CompareTo(addressList[b].Length);
			});

			Node root = new Node("");
			foreach(int index in indices) {
				Node current = root;
				foreach(string path in addressList[index]) {
					Node next = current.Find(path);
					if (next == null) {
						next = current.Add(path);
					}
					current = next;
				}
			}

			using (Namespace("hexegeer")) {
				using (Class("ResourceAddressList")) {
					foreach(int index in indices) {
						string[] address = addressList[index];
						string path = string.Join('_', address).ToUpper();
						AppendLine($"public const string {path} = \"{rawAddressList[index]}\";");
					}

					foreach(Node node in root.children) {
						WriteBlock(node, addressList);
					}
				}
			}
		}

		private void WriteBlock(Node node, in List<string[]> addressList) {
			if (node.children.Count == 0) {
			} else {
				string address = node.address;
				if (!string.IsNullOrEmpty(address)) {
					if ('a' <= address[0] && address[0] <= 'z') {
						if (address.Length > 1) {
							address = (char)(address[0] - 'a' + 'A') + address.Substring(1);
						} else {
							address = $"{(char)(address[0] - 'a' + 'A')}";
						}
					}
					using (Class(address, isStatic: true)) {
						ChildrenAddress(node, addressList);
						foreach(Node child in node.children) {
							WriteBlock(child, addressList);
						}
					}
				}
			}
		}

		private void ChildrenAddress(Node node, in List<string[]> addressList) {
			List<string> targetPath = new List<string>();
			Node current = node;

			while(current.parent != null) {
				targetPath.Insert(0, current.address);
				current = current.parent;
			}

			List<string[]> childList = addressList.FindAll(_ => {
				if (targetPath.Count < _.Length) {
					for (int i = 0; i < targetPath.Count; ++i) {
						if (targetPath[i] != _[i]) { return false; }
					}
					return true;
				} else {
					return false;
				}
			});

			AppendLine($"public static readonly string[] list = new string[] {{");
			using (Indent) {
				foreach(string[] child in childList) {
					AppendLine($"{string.Join('_', child).ToUpper()},");
				}
			}
			AppendLine($"}};");
		}
	}
}