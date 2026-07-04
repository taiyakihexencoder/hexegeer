using System.Collections.Generic;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	/// <summary>
	/// 選択肢の更新を一元管理するPopupField。
	/// 複数のPopupFieldを１つのリストに対応させ、
	/// 変更を検知して選択肢を更新する。
	/// </summary>
	/// <typeparam name="KEY"></typeparam>
	public class ListPopupBuilder<KEY> {
		private List<KEY> _keys;
		private System.Func<KEY, string> _nameConverter;

		private System.Action onDictionaryUpdated;

		public ListPopupBuilder() {
			_keys = new List<KEY>();
			_nameConverter = DefaultCoverter;
		}

		/// <summary>
		/// PopupFieldを作成。
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public PopupField<KEY> Generate(KEY defaultValue) {
			PopupField<KEY> popup = new PopupField<KEY>(
				choices: _keys,
				defaultIndex: _keys.FindIndex(_ => _.Equals(defaultValue)),
				formatListItemCallback: _nameConverter,
				formatSelectedValueCallback: _nameConverter
			);

			System.Action onUpdated = () => {
				popup.choices = _keys;
				popup.formatListItemCallback = _nameConverter;
				popup.formatSelectedValueCallback = _nameConverter;
			};
			onDictionaryUpdated += onUpdated;

			EventCallback<DetachFromPanelEvent> detach = default;
			detach = evt => {
				onDictionaryUpdated -= onUpdated;
			};
			popup.RegisterCallback(detach);

			EventCallback<AttachToPanelEvent> attach = default;
			attach = evt => {
				onDictionaryUpdated += onUpdated;
			};
			popup.RegisterCallback(attach);

			return popup;
		}

		/// <summary>
		/// 辞書の更新。
		/// 更新によってUIの選択肢も更新される。
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public ListPopupBuilder<KEY> SetKeys(List<KEY> keys) {
			_keys = keys;
			onDictionaryUpdated?.Invoke();
			return this;
		}

		/// <summary>
		/// パラメーターとテキストの対応関係をセット。
		/// </summary>
		/// <param name="converter"></param>
		/// <returns></returns>
		public ListPopupBuilder<KEY> SetConverter(System.Func<KEY, string> converter) {
			_nameConverter = converter ?? DefaultCoverter;
			return this;
		}

		private string DefaultCoverter(KEY key) {
			return key.ToString();
		}
	}
}