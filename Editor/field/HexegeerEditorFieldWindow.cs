using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorFieldWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Field");

			VisualElement mainView = CreateMainView();
			rootVisualElement.Add(mainView);
		}

		private VisualElement CreateMainView() {
			FieldMainSettings mainSettings = FieldMainSettings.instance;

			ScrollPane frame = new ScrollPane()
				.Padding(vertical: 16, horizontal: 24);

			frame.Add(Text.H2("Editor Mode"));

			EnumField field = new EnumField(mainSettings.ViewType);
			field.RegisterValueChangedCallback(v => {
				mainSettings.ViewType = (FieldViewType) v.newValue;
			});

			frame.Add(field);

			switch (mainSettings.ViewType) {
				case FieldViewType.SideView: {
					frame.Add(SideViewSettingsView());
					break;
				}
			}

			ClickButton button = ClickButton.Create()
				.Label("Generate Runtime Resource");
			button.OnClicked += OnRequestGenerate;

			frame.Add(button);

			return frame;
		}

		private VisualElement SideViewSettingsView() {
			FieldSideViewSettings settings = FieldSideViewSettings.instance;

			internallib.Column column = new internallib.Column()
				.Padding(vertical: 12, horizontal: 16);

			column.AddChildren( Text.H2("Side View Settings") );

			internallib.Column internalColumn = new internallib.Column()
				.Padding(horizontal: 16);
			column.AddChildren(internalColumn);
			
			EditFloat widthField = new EditFloat(settings.Width)
				.WidthPercent(50)
				.OnChanged(value => settings.Width = value)
				.Label("Width");

			EditFloat offsetField = new EditFloat(settings.ZOffset)
				.WidthPercent(50)
				.OnChanged(value => settings.ZOffset = value)
				.Label("Offset");

			internalColumn.AddChildren(widthField, offsetField);

			return column;
		}

		private void OnRequestGenerate() {

		}
	}
}