using System.Collections.Generic;
using System.IO;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorEventPointWindow : EditorWindow {
		private List<bool> _descriptionVisibility;

		private void OnEnable() {
			titleContent = new GUIContent("Event Point");

			ScrollPane pane = new ScrollPane()
				.Margin(horizontal:24f, vertical:12f);
			rootVisualElement.Add(pane);

			_descriptionVisibility = new List<bool>();

			CreateView(pane);
		}

		private void CreateView(ScrollPane pane) {
			pane.Clear();


			EventPointSettings settings = EventPointSettings.instance;

			Row titleRow = new Row().WidthPercent(90f);
			ClickButton scriptGenerateButton = ClickButton.Create()
				.Label("Generate Script");
			scriptGenerateButton.OnClicked += () => {
				EventPointScriptGenerator generator = new EventPointScriptGenerator();
				if (generator.Validation(out List<string> messages)) {
					generator.Generate($"eventpoint{Path.DirectorySeparatorChar}EventPointPartials.cs");
				} else {
					EditorUtility.DisplayDialog("Error", string.Join(System.Environment.NewLine, messages), "Ok");
				}
			};

			titleRow.AddChildren(
				Text.H2("Event Point"), 
				new Spacer().Weight(1f),
				scriptGenerateButton
			);
			pane.Add(titleRow);

			for (int i = 0; i < settings.Rows.Count; ++i) {
				while (_descriptionVisibility.Count <= i) {
					_descriptionVisibility.Add(false);
				}

				int index = i;
				Row row = new Row()
					.WidthPercent(90f);

				Text descriptionButton = Text.Body("〇");
				descriptionButton.RegisterCallback<MouseDownEvent>(evt => {
					if (evt.button == 0) {
						_descriptionVisibility[index] = !_descriptionVisibility[index];
						CreateView(pane);
					}
				});

				TextField nameField = new TextField();
				nameField.SetValueWithoutNotify(settings.Rows[i].name);
				nameField.RegisterValueChangedCallback(v => {
					settings.Rows[index].name = v.newValue;
					settings.UpdateParameter(index, settings.Rows[index]);
				});
				nameField.style.flexBasis = 0f;
				nameField.style.flexGrow = 1f;

				ClickButton moveUpButton = ClickButton.Create()
					.Label("↑");
				moveUpButton.OnClicked += () => {
					bool temp = _descriptionVisibility[index];
					_descriptionVisibility[index] = _descriptionVisibility[index-1];
					_descriptionVisibility[index-1] = temp;

					settings.MoveUp(index);
					CreateView(pane);
				};
				moveUpButton.enabledSelf = index > 0;

				ClickButton moveDownButton = ClickButton.Create()
					.Label("↓");
				moveDownButton.OnClicked += () => {
					bool temp = _descriptionVisibility[index];
					_descriptionVisibility[index] = _descriptionVisibility[index+1];
					_descriptionVisibility[index+1] = temp;

					settings.MoveDown(index);
					CreateView(pane);
				};
				moveDownButton.enabledSelf = index < settings.Rows.Count-1;

				ClickButton deleteButton = ClickButton.Create()
					.Label("-");
				deleteButton.OnClicked += () => {
					_descriptionVisibility.RemoveAt(index);

					settings.RemoveAt(index);
					CreateView(pane);
				};

				row.AddChildren(
					descriptionButton,
					nameField,
					Text.Body("ID="),
					Text.Body(settings.Rows[index].eventId.ToString()).Width(40f),
					new Spacer().Weight(1f),
					moveUpButton,
					new Spacer(width:12f),
					moveDownButton,
					new Spacer(width:12f),
					deleteButton
				);

				Text descriptionText = Text.Body("description")
					.FontSize(10f)
					.Bold();
				descriptionText.style.display = _descriptionVisibility[i] ? DisplayStyle.Flex : DisplayStyle.None;

				TextField descriptionField = new TextField();
				descriptionField.multiline = true;
				descriptionField.maxLength = 200;
				descriptionField.isDelayed = true;
				descriptionField.SetValueWithoutNotify(settings.Rows[i].description);
				descriptionField.RegisterValueChangedCallback(v => {
					settings.Rows[index].description = v.newValue;
					settings.UpdateParameter(index, settings.Rows[index]);
				});
				descriptionField.style.width = new Length(90, LengthUnit.Percent);
				descriptionField.style.minHeight = 60f;
				descriptionField.style.display = _descriptionVisibility[i] ? DisplayStyle.Flex : DisplayStyle.None;
				descriptionField.style.marginBottom = 12f;

				pane.AddChildren(row, descriptionText, descriptionField);
			}
			pane.Add(new Spacer(height: 20f));

			ClickButton addButton = ClickButton.Create()
				.Label("+");
			addButton.OnClicked += () => {
				_descriptionVisibility.Add(false);

				settings.Add();
				CreateView(pane);
			};
			pane.Add(addButton);
		}
	}
}