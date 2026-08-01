using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	/// <summary>
	/// Addressablesに設定したオブジェクトのセレクター。
	/// typeはComponentならGameObjectかつそのコンポーネントを保持しているかで判定。
	/// </summary>
	public class AddressableListPopupBuilder {
		private Dictionary<string, string> _values;

		private AddressableAssetSettings _settings;

		public AddressableListPopupBuilder(string group, System.Type type) : this(
			targetGroup: _ => _ == group,
			targetAsset: _ => IsTargetType(_, type),
			targetAddress: _ => true,
			conversion: DefaultConversion
		) { }

		public AddressableListPopupBuilder(string rootPath) : this(
			targetGroup: _ => true,
			targetAsset: _ => true,
			targetAddress: _ => _.StartsWith($"{rootPath}/"),
			conversion: (address, asset) => address.Substring(rootPath.Length+1)
		) { }

		public AddressableListPopupBuilder(System.Type type) : this(
			targetGroup: _ => true,
			targetAsset: _ => IsTargetType(_, type),
			targetAddress: _ => true,
			conversion: DefaultConversion
		) { }

		public AddressableListPopupBuilder(System.Type type, string rootPath) : this(
			targetGroup: _ => true,
			targetAsset: _ => IsTargetType(_, type),
			targetAddress: _ => _.StartsWith($"{rootPath}/"),
			conversion: (address, asset) => address.Substring(rootPath.Length+1)
		) { }

		public AddressableListPopupBuilder(
			System.Predicate<string> targetGroup,
			System.Predicate<Object> targetAsset,
			System.Predicate<string> targetAddress,
			System.Func<string, Object, string> conversion
		) {
			_values = new Dictionary<string, string> {
				{ "", " - " }
			};
			_settings = AddressableAssetSettingsDefaultObject.GetSettings(true);

			foreach(AddressableAssetGroup group in _settings.groups) {
				if (targetGroup(group.name)) {
					foreach(AddressableAssetEntry entry in group.entries) {
						if (targetAsset(entry.TargetAsset) && targetAddress(entry.address) ) {
							_values.Add(entry.address, conversion(entry.address, entry.TargetAsset));
						}
					}
				}
			}
		}

		public PopupField<string> Generate(string defaultValue) {
			List<string> keys = new List<string>(_values.Keys);
			PopupField<string> popup = new PopupField<string>(
				choices: keys,
				defaultIndex: keys.FindIndex(_ => _ == defaultValue),
				formatListItemCallback: _ => _values.TryGetValue(_, out string address) ? address : "-",
				formatSelectedValueCallback: _ => _
			);

			return popup;
		}

		private static bool IsTargetType(Object obj, System.Type targetType) {
			if (targetType.IsSubclassOf(typeof(Component))) {
				return obj is GameObject go && go.TryGetComponent(targetType, out _);
			} else {
				return targetType.IsAssignableFrom(obj.GetType());
			}
		}

		private static string DefaultConversion(string address, Object obj) {
			return address;
		}
	}
}