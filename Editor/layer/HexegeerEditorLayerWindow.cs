using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorLayerWindow : EditorWindow {
		private void OnEnable() {
			titleContent = new GUIContent("Layer");
			rootVisualElement.Add(CreateView());
		}

		private VisualElement CreateView() {
			int defaultLayerCount = System.Enum.GetValues(typeof(DefaultLayer)).Length;

			LayerSettings settings = LayerSettings.instance;

			ScrollPane pane = new ScrollPane()
				.Padding(horizontal: 24f);

			pane.Add(new Spacer(height:24f));

			for (int i = 0; i < LayerSettings.LAYER_COUNT; ++i) {
				int fromIndex = settings.LayerIndex(i);
				Row row = new Row();

				string fromName = settings.LayerName(i);

				int index = i;
				TextField nameField = new TextField();
				nameField.enabledSelf = i >= defaultLayerCount;
				nameField.isDelayed = true;
				nameField.SetValueWithoutNotify(fromName);
				nameField.RegisterValueChangedCallback(v => {
					settings.LayerName(index, v.newValue);
					rootVisualElement.Clear();
					rootVisualElement.Add(CreateView());
				});
				nameField.style.fontSize = 10f;
				nameField.style.width = new StyleLength(new Length(20, LengthUnit.Percent));
				nameField.style.height = 14f;
				nameField.style.paddingRight = 8f;
				nameField.style.marginTop = 0f;
				nameField.style.marginBottom = 0f;
				row.Add(nameField);

				Label label = new Label(i.ToString("00"));
				label.style.fontSize = 8f;
				label.style.width = 16f;
				label.style.unityTextAlign = TextAnchor.MiddleRight;
				row.Add(label);

				for (int j = 0; j <= i; ++j) {
					int toIndex = settings.LayerIndex(j);
					Toggle toggle = new Toggle();
					toggle.focusable = false;
					toggle.enabledSelf = i >= defaultLayerCount && j >= defaultLayerCount 
						&& !string.IsNullOrEmpty(fromName) && !string.IsNullOrEmpty(settings.LayerName(j));
					toggle.style.marginLeft = 0f;
					toggle.style.marginRight = 0f;
					toggle.style.marginTop = 0f;
					toggle.style.marginBottom = 0f;
					toggle.SetValueWithoutNotify(settings.Table(fromIndex, toIndex));
					toggle.RegisterValueChangedCallback(v => {
						settings.Table(fromIndex, toIndex, v.newValue);
					});
					row.Add(toggle);
				}

				pane.Add(row);
			}

			Row nameRow = new Row();
			nameRow.style.paddingTop = 4f;
			nameRow.style.paddingBottom = 80f;
			nameRow.style.paddingLeft = 13f;
			nameRow.style.marginLeft = new StyleLength(new Length(20, LengthUnit.Percent));
			StyleTransformOrigin origin = new StyleTransformOrigin(new TransformOrigin(5f, 5f));
			for(int i = 0; i < LayerSettings.LAYER_COUNT; ++i) {
				Label name = new Label(settings.LayerName(i));
				name.style.flexBasis = 0f;
				name.style.flexGrow = 0f;
				name.style.fontSize = 10.5f;
				name.style.marginLeft = 11f;
				name.style.transformOrigin = origin;
				name.style.rotate = new StyleRotate(new Rotate(90f, Vector3.forward));
				nameRow.Add(name);
			}
			pane.Add(nameRow);

			pane.Add(new Spacer(height:16f));

			ClickButton button = ClickButton.Create()
				.Label("Generate Script");

			button.OnClicked += GenerateScript;
			pane.Add(button);

			pane.Add(new Spacer(height:24f));

			return pane;
		}

		private void GenerateScript() {
			SourceCodeGenerator generator = new LayerScriptGenerator();

			if (generator.Validation(out List<string> errorMessages)) {
				generator.Generate($"layer{FileUtil.Sep}Layer.cs");
			} else {
				EditorUtility.DisplayDialog("エラー", string.Join(FileUtil.Lb, errorMessages), "ok");
			}
		}
	}
}
