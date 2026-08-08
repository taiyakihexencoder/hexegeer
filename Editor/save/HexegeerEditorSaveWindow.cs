using System.Collections.Generic;
using System.IO;
using hexegeer.internallib;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorSaveWindow : EditorWindow {
		private enum Mode {
			Unselected = -1,
			Global,
			User,
		}

		private Mode mode;

		private void OnEnable() {
			titleContent = new GUIContent("Save");
			ScrollPane pane = new ScrollPane()
				.Padding(16f);

			mode = Mode.Global;

			CreateView(pane);
			rootVisualElement.Add(pane);
		}

		private void CreateView(ScrollPane pane) {
			pane.Clear();

			ClickButton generateButton = ClickButton.Create()
				.Label("Generate Script")
				.Margin(12f);
			generateButton.OnClicked += () => {
				SaveScriptGenerator generator = new SaveScriptGenerator();
				if (generator.Validation(out List<string> messages)) {
					generator.Generate($"save{Path.DirectorySeparatorChar}Partial.cs");
				} else {
					EditorUtility.DisplayDialog("Error", string.Join(System.Environment.NewLine, messages), "ok");
				}
			};
			pane.Add(generateButton);

			ScrollPane mainPane = new ScrollPane();

			Tabs tabs = new Tabs((int)mode);
			pane.Add(tabs);
			tabs.Add("GLOBAL", "USER");
			tabs.SelectedIndexChanged += (index) => {
				Mode updatedMode;
				if (index == (int) Mode.Global) { updatedMode = Mode.Global; }
				else if (index == (int) Mode.User) { updatedMode = Mode.User; }
				else { updatedMode = Mode.Unselected; }
				if (updatedMode != mode) {
					mode = updatedMode;
					UpdateMainPane(mainPane);
				}
			};

			pane.Add(mainPane);
			UpdateMainPane(mainPane);
		}

		private void UpdateMainPane(VisualElement mainPane) {
			switch(mode) {
				case Mode.Global: {
					CreateGlobalParameterView(mainPane);
					break;
				}
				case Mode.User: {
					CreateUserParameterView(mainPane);
					break;
				}
			}
		}

		private void CreateGlobalParameterView(VisualElement pane) {
			pane.Clear();
			SaveSettings settings = SaveSettings.instance;
			List<SaveSettings.SaveParameter> parameters = settings.Global.parameters;

			Text header = Text.H3("Global");
			pane.Add(header);

			for (int i = 0; i < parameters.Count; ++i) {
				VisualElement row = ParameterRow(
					parameter: parameters[i],
					index: i,
					isLast: i == parameters.Count-1,
					update: (parameter, index) => settings.UpdateGlobalParameter(index, parameter),
					remove: (index) => settings.RemoveGlobalParameter(index),
					refresh: () => CreateGlobalParameterView(pane),
					moveUp: (index) => settings.MoveUpGlobal(index),
					moveDown: (index) => settings.MoveDownGlobal(index)
				);
				pane.Add(row);
			}

			ClickButton addButton = ClickButton.Create()
				.Label("+");
			addButton.OnClicked += () => {
				settings.AddGlobalParameter();
				CreateGlobalParameterView(pane);
			};
			pane.Add(addButton);
		}

		private void CreateUserParameterView(VisualElement pane) {
			pane.Clear();
			SaveSettings settings = SaveSettings.instance;
			List<SaveSettings.SaveParameter> parameters = settings.User.parameters;

			Text header = Text.H3("User");
			pane.Add(header);

			for (int i = 0; i < parameters.Count; ++i) {
				VisualElement row = ParameterRow(
					parameter: parameters[i],
					index: i,
					isLast: i == parameters.Count-1,
					update: (parameter, index) => settings.UpdateUserParameter(index, parameter),
					remove: (index) => settings.RemoveUserParameter(index),
					refresh: () => CreateUserParameterView(pane),
					moveUp: (index) => settings.MoveUpUser(index),
					moveDown: (index) => settings.MoveDownUser(index)
				);
				pane.Add(row);
			}
			
			ClickButton addButton = ClickButton.Create()
				.Label("+");
			addButton.OnClicked += () => {
				settings.AddUserParameter();
				CreateUserParameterView(pane);
			};
			pane.Add(addButton);
		}

		private VisualElement ParameterRow(
			SaveSettings.SaveParameter parameter, 
			int index,
			bool isLast,
			System.Action<SaveSettings.SaveParameter, int> update,
			System.Action<int> remove,
			System.Action refresh,
			System.Action<int> moveUp,
			System.Action<int> moveDown
		) {
			Row row = new Row();
			TextField nameField = new TextField();
			nameField.isDelayed = true;
			nameField.style.flexBasis = 0f;
			nameField.style.flexGrow = 2f;
			nameField.SetValueWithoutNotify(parameter.name);
			nameField.RegisterValueChangedCallback(v => {
				parameter.name = v.newValue;
				update(parameter, index);
			});

			EnumField typeField = new EnumField(parameter.type);
			typeField.style.flexBasis = 0f;
			typeField.style.flexGrow = 1f;
			typeField.SetValueWithoutNotify(parameter.type);
			typeField.RegisterValueChangedCallback(v => {
				parameter.type = (SaveSettings.SaveParameterType)v.newValue;
				parameter.defaultValue = GetDefault((SaveSettings.SaveParameterType)v.newValue);
				update(parameter, index);
				refresh();
			});

			VisualElement parameterField = ParameterValueElement(
				parameter.type, 
				parameter.defaultValue, 
				text => {
					parameter.defaultValue = text;
					update(parameter, index);
				}
			);
			parameterField.style.flexBasis = 0f;
			parameterField.style.flexGrow = 2f;

			ClickButton moveUpButton = ClickButton.Create()
				.Label("↑");
			moveUpButton.enabledSelf = index > 0;
			moveUpButton.OnClicked += () => {
				moveUp(index);
				refresh();
			};

			ClickButton moveDownButton = ClickButton.Create()
				.Label("↓");
			moveDownButton.enabledSelf = !isLast;
			moveDownButton.OnClicked += () => {
				moveDown(index);
				refresh();
			};
			ClickButton removeButton = ClickButton.Create()
				.Label("-");
			removeButton.OnClicked += () => {
				remove(index);
				refresh();
			};

			row.AddChildren(nameField, typeField, parameterField, moveUpButton, moveDownButton, removeButton);

			return row;
		}

		private string GetDefault(SaveSettings.SaveParameterType type) {
			switch (type) {
				case SaveSettings.SaveParameterType.Int: return "0";
				case SaveSettings.SaveParameterType.Long: return "0";
				case SaveSettings.SaveParameterType.Boolean: return "false";
				case SaveSettings.SaveParameterType.String: return "";
				case SaveSettings.SaveParameterType.Float: return "0.0f";
				case SaveSettings.SaveParameterType.Vector2: return "0.0f,0.0f";
				case SaveSettings.SaveParameterType.Vector3: return "0.0f,0.0f,0.0f";
				case SaveSettings.SaveParameterType.Color: return "0.0f,0.0f,0.0f,1.0f";
				default: return "";
			}
		}

		private VisualElement ParameterValueElement(
			SaveSettings.SaveParameterType type,
			string value,
			System.Action<string> updated
		) {
			VisualElement element = null;
			switch (type) {
				case SaveSettings.SaveParameterType.Int: {
					IntegerField intField = new IntegerField();
					intField.RegisterValueChangedCallback(v => updated(v.newValue.ToString()));
					intField.SetValueWithoutNotify(int.TryParse(value, out int v) ? v : 0);
					element = intField;
					break;
				}
				case SaveSettings.SaveParameterType.Long: {
					LongField longField = new LongField();
					longField.RegisterValueChangedCallback(v => updated(v.newValue.ToString()));
					longField.SetValueWithoutNotify(long.TryParse(value, out long v) ? v : 0);
					element = longField;
					break;
				}
				case SaveSettings.SaveParameterType.Boolean: {
					Toggle toggle = new Toggle();
					toggle.RegisterValueChangedCallback(v => updated(v.newValue ? "true" : "false"));
					toggle.SetValueWithoutNotify(value == "true");
					element = toggle;
					break;
				}
				case SaveSettings.SaveParameterType.String: {
					TextField textField = new TextField();
					textField.RegisterValueChangedCallback(v => updated(v.newValue));
					textField.SetValueWithoutNotify(value);
					element = textField;
					break;
				}
				case SaveSettings.SaveParameterType.Float: {
					FloatField floatField = new FloatField();
					floatField.RegisterValueChangedCallback(v => updated(v.newValue.ToString()+"f"));
					floatField.SetValueWithoutNotify(float.TryParse(value.Replace("f", ""), out float v) ? v : 0.0f);
					element = floatField;
					break;
				}
				case SaveSettings.SaveParameterType.Vector2: {
					Vector2Field vectorField = new Vector2Field();
					vectorField.RegisterValueChangedCallback(v => updated($"{v.newValue.x}f,{v.newValue.y}f"));
					string[] split = value.Replace("f", "").Split(',');
					if (split.Length == 2 && float.TryParse(split[0], out float x) && float.TryParse(split[1], out float y)) {
						vectorField.SetValueWithoutNotify(new Vector2(x, y));
					} else {
						vectorField.SetValueWithoutNotify(Vector2.zero);
					}
					element = vectorField;
					break;
				}
				case SaveSettings.SaveParameterType.Vector3: {
					Vector3Field vectorField = new Vector3Field();
					vectorField.RegisterValueChangedCallback(v => updated($"{v.newValue.x}f,{v.newValue.y}f,{v.newValue.z}f"));
					string[] split = value.Replace("f", "").Split(',');
					if (split.Length == 3 && float.TryParse(split[0], out float x) && float.TryParse(split[1], out float y) && float.TryParse(split[2], out float z)) {
						vectorField.SetValueWithoutNotify(new Vector3(x, y, z));
					} else {
						vectorField.SetValueWithoutNotify(Vector3.zero);
					}
					element = vectorField;
					break;
				}
				case SaveSettings.SaveParameterType.Color: {
					ColorField colorField = new ColorField();
					colorField.RegisterValueChangedCallback(v => updated($"{v.newValue.r}f,{v.newValue.g}f,{v.newValue.b}f,{v.newValue.a}f"));
					string[] split = value.Replace("f", "").Split(',');
					if (split.Length == 4 && float.TryParse(split[0], out float r) && float.TryParse(split[1], out float g) && float.TryParse(split[2], out float b) && float.TryParse(split[3], out float a)) {
						colorField.SetValueWithoutNotify(new Color(r, g, b, a));
					} else {
						colorField.SetValueWithoutNotify(new Color(0f, 0f, 0f, 1f));
					}
					element = colorField;
					break;
				}
				default: {
					break;
				}
			}

			if (element == null) {
				element = new VisualElement();
			}

			return element;
		}
	}
}
