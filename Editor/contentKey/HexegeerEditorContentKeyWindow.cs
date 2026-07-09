using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	using System.Collections.Generic;
	using hexegeer.internallib;

	public sealed class HexegeerEditorContentKeyWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Content Key");

			rootVisualElement.Add(CreateView());
		}

		private VisualElement CreateView() {
			ScrollPane pane = new ScrollPane()
				.Padding(horizontal: 24f);

			pane.Add(new Spacer(height:24f));

			VisualElement keyList = new VisualElement();
			CreateKeyList(keyList);

			pane.Add(keyList);

			pane.Add(new Spacer(height: 24f));
			return pane;
		}

		private void CreateKeyList(VisualElement parent) {
			parent.Clear();

			ContentKeySetting settings = ContentKeySetting.instance;
			List<ContentKeySetting.Key> keys = settings.Keys;

			{
				Row globalRow = new Row();
				TextField globalField = new TextField();
				globalField.style.width = 200;
				globalField.SetValueWithoutNotify("Global");
				globalField.enabledSelf = false;

				ClickButton dummyButton = ClickButton.Create()
					.Label("-");
				dummyButton.enabledSelf = false;

				globalRow.AddChildren(globalField, dummyButton);

				parent.Add(globalRow);
			}

			for(int i = 0; i < keys.Count; ++i) {
				int index = i;
				Row row = new Row();

				TextField nameField = new TextField();
				nameField.style.width = 200;
				nameField.SetValueWithoutNotify(keys[index].name);
				nameField.RegisterValueChangedCallback( v =>{
					settings.SetName(keys[index], v.newValue);
				});

				ClickButton minusButton = ClickButton.Create()
					.Label("-");
				minusButton.OnClicked += () => {
					settings.Remove(keys[index].id);
					CreateKeyList(parent);
				};

				row.AddChildren(nameField, minusButton);

				parent.Add(row);
			}

			ClickButton plusButton = ClickButton.Create()
				.Label("+");
			plusButton.OnClicked += () => {
				settings.Add("New Content Key");
				CreateKeyList(parent);
			};

			parent.Add(plusButton);
		
		}
	}
}
