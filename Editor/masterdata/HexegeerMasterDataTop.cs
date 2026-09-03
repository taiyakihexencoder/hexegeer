using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public class HexegeerMasterDataTop : EditorWindow {
		private TwoPaneSplitView _mainView;
		private SelectableList<HexegeerMasterDataSettings.DataClass> _listView; 
		private ScrollPane _detailView;

		private List<HexegeerMasterDataTable> _windows;
		private Regex _regexFileName = new Regex(@"^[a-zA-Z0-9\.\-_ ]*$");

		private void OnEnable() {
			titleContent = new GUIContent("Top");
			
			_windows = new List<HexegeerMasterDataTable>();
			_mainView = new TwoPaneSplitView(0, 250f, TwoPaneSplitViewOrientation.Horizontal);
			
			ScrollPane listPane = new ScrollPane();
			_listView = new SelectableList<HexegeerMasterDataSettings.DataClass>();
			CreateListView();

			ClickButton generateButton = ClickButton.Create()
				.Label("Generate Script")
				.Margin(8f);
			generateButton.OnClicked += () => {
				HexegeerMasterDataScriptGenerator generator = new HexegeerMasterDataScriptGenerator();
				if (generator.Validation(out List<string> messages)) {
					generator.Generate($"masterdata{Path.DirectorySeparatorChar}MasterData.cs");
				} else {
					EditorUtility.DisplayDialog("Error", $"{string.Join(System.Environment.NewLine, messages)}", "Ok");
				}
			};

			ClickButton addButton = ClickButton.Create()
				.Label("+")
				.Margin(8f);
			addButton.OnClicked += () => {
				HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
				settings.AddClass();
				OpenTableWindow(settings.ClassList[settings.ClassList.Count-1]);
				CreateListView();
			};

			listPane.Add(generateButton);
			listPane.Add(Text.Body("Tables").Margin(8f));
			listPane.Add(_listView);
			listPane.Add(addButton);

			_mainView.Add(listPane);

			_detailView = new ScrollPane()
				.Padding(horizontal:24f, vertical: 12f);
			_mainView.Add(_detailView);

			rootVisualElement.Add(_mainView);

			EditorApplication.delayCall += () => {
				List<HexegeerMasterDataTable> windowList = new List<HexegeerMasterDataTable>(Resources.FindObjectsOfTypeAll<HexegeerMasterDataTable>());
				foreach (HexegeerMasterDataSettings.DataClass data in HexegeerMasterDataSettings.instance.ClassList) {
					if (!windowList.Exists(_ => _.Id == data.id)) {
						OpenTableWindow(data);
					}
				}
				Focus();
			};
		}

		private void OpenTableWindow(HexegeerMasterDataSettings.DataClass data) {
			_windows.Add(HexegeerMasterDataTable.Open(data.id));
			Focus();
		}

		private void CloseTableWindow(int index) {
			_windows[index].Close();
			_windows.RemoveAt(index);
		}

		private void CreateListView() {
			HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
			_listView.ClearElements();

			for (int i = 0; i < settings.ClassList.Count; ++i) {
				int index = i;

				Row row = new Row()
					.Padding(horizontal: 4f, vertical: 8f);
				Text text = Text.Body(settings.ClassList[i].className);

				ClickButton moveUpButton = ClickButton.Create()
					.Circle(24f)
					.Margin(horizontal:4f)
					.Label("↑");
				moveUpButton.OnClicked += () => {
					if (index > 0) {
						HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
						settings.MoveUpClass(index);
					}
				};

				ClickButton moveDownButton = ClickButton.Create()
					.Circle(24f)
					.Margin(horizontal:4f)
					.Label("↓");
				moveDownButton.OnClicked += () => {
					HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
					if (index < settings.ClassList.Count-1) {
						settings.MoveDownClass(index);
					}
				};

				ClickButton deleteButton = ClickButton.Create()
					.Circle(24f)
					.Margin(horizontal:4f)
					.Label("×");
				deleteButton.OnClicked += () => {
					HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
					settings.RemoveClass(index);
					CreateListView();
					CloseTableWindow(index);
				};

				row.AddChildren(
					text,
					new Spacer().Weight(1f),
					moveUpButton,
					moveDownButton,
					deleteButton
				);

				_listView.AddSelection(
					settings.ClassList[i],
					row
				);
			}

			_listView.selectionChanged += (data) => {
				if (data != null) {
					CreateDetailView(data);
				} else {
					_detailView.Clear();
				}
			};
		}

		private void CreateDetailView(HexegeerMasterDataSettings.DataClass data) {
			_detailView.Clear();

			Row classNameRow = new Row().Padding(12f);

			int classIndex = HexegeerMasterDataSettings.instance.ClassList.FindIndex(_ => _.className == data.className);

			TextField classNameField = new TextField();
			classNameField.style.flexBasis = 0f;
			classNameField.style.flexGrow = 1f;
			classNameField.isDelayed = true;
			classNameField.SetValueWithoutNotify(data.className);
			classNameField.RegisterValueChangedCallback(v => {
				HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
				if (classIndex >= 0) {
					settings.UpdateClassName(classIndex, v.newValue);
					CreateListView();
					_listView.Select(data);
					_windows[classIndex].titleContent = new GUIContent(v.newValue);
				}
			});

			classNameRow.AddChildren(Text.Body("Class"), new Spacer(width:16f), classNameField);
			_detailView.Add(classNameRow);

			Row fileNameRow = new Row().Padding(12f);

			TextField fileNameField = new TextField();
			fileNameField.style.flexBasis = 0f;
			fileNameField.style.flexGrow = 1f;
			fileNameField.isDelayed = true;
			fileNameField.SetValueWithoutNotify(data.fileName);
			fileNameField.RegisterValueChangedCallback(v => {
				HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
				if (classIndex >= 0) {
					string newFileName = v.newValue;
					string oldFileName = settings.ClassList[classIndex].fileName;
					if (_regexFileName.IsMatch(newFileName)) {
						if (UpdateFileName(oldFileName, newFileName)) {
							settings.UpdateFileName(classIndex, newFileName);
						} else {
							EditorUtility.DisplayDialog("Error", "Fail file name update.(already exists?)", "ok");
							fileNameField.SetValueWithoutNotify(oldFileName);
						}
					} else {
						EditorUtility.DisplayDialog("Validation", "Following characters are allowed: a-zA-Z0-9.-_ ", "ok");
						fileNameField.SetValueWithoutNotify(oldFileName);
					}
				}
			});

			fileNameRow.AddChildren(Text.Body("File name"), new Spacer(width:16f), fileNameField);
			_detailView.Add(fileNameRow);

			_detailView.Add(Text.Body("Parameters").Padding(horizontal:12f));
			for(int i = 0; i < data.columns.Count; ++i) {
				int index = i;
				Row row = new Row().Padding(horizontal: 24f);

				TextField nameField = new TextField();
				nameField.style.flexBasis = 0f;
				nameField.style.flexGrow = 1f;
				nameField.SetValueWithoutNotify(data.columns[i].Name);
				nameField.RegisterValueChangedCallback(v => {
					HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
					if (classIndex >= 0) {
						EditorGridView.Column column = settings.ClassList[classIndex].columns[index];
						column.Name = v.newValue;
						settings.UpdateColumn(classIndex, index, column);
					}
				});

				EnumField typeField = new EnumField(data.columns[i].Type);
				typeField.style.width = 80f;
				typeField.RegisterValueChangedCallback(v => {
					HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
					if (classIndex >= 0) {
						EditorGridView.Column column = settings.ClassList[classIndex].columns[index];
						column.Type = (EditorGridView.ColumnType) v.newValue;
						settings.UpdateColumn(classIndex, index, column);
					}
				});

				ClickButton moveUpButton = ClickButton.Create()
					.Margin(horizontal: 4f)
					.Circle(24f)
					.Label("↑");
				moveUpButton.OnClicked += () => {
					if (classIndex >= 0 && index > 0) {
						HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
						settings.MoveUpColumn(classIndex, index);
						CreateDetailView(settings.ClassList[classIndex]);
					}
				};

				ClickButton moveDownButton = ClickButton.Create()
					.Margin(horizontal: 4f)
					.Circle(24f)
					.Label("↓");
				moveDownButton.OnClicked += () => {
					if (classIndex >= 0 && index < data.columns.Count-1) {
						HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
						settings.MoveDownColumn(classIndex, index);
						CreateDetailView(settings.ClassList[classIndex]);
					}
				};
				ClickButton deleteButton = ClickButton.Create()
					.Margin(horizontal: 4f)
					.Circle(24f)
					.Label("×");
				deleteButton.OnClicked += () => {
					if (classIndex >= 0) {
						HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
						settings.RemoveColumn(classIndex, index);
						CreateDetailView(settings.ClassList[classIndex]);
					}
				};

				row.AddChildren(
					nameField,
					typeField,
					new Spacer(width:24f),
					moveUpButton, 
					moveDownButton, 
					deleteButton
				);
				_detailView.Add(row);
			}
			ClickButton addButton = ClickButton.Create(Align.FlexEnd)
				.Circle(32f)
				.Label("+");
			addButton.OnClicked += () => {
				if (classIndex >= 0) {
					HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
					settings.AddColumn(classIndex);
					CreateDetailView(settings.ClassList[classIndex]);
				}
			};

			_detailView.Add(addButton);
		}

		private bool UpdateFileName(string oldName, string newName) {
			string path = $"{Application.dataPath}{Path.DirectorySeparatorChar}{HexegeerMasterDataTable.TablePath}";
			if (!File.Exists($"{path}{oldName}")) {
				// ファイルがない場合はセーブ時に新規作成なので書き換えの必要なし。
				return true;
			} else {
				try {
					File.Move($"{path}{oldName}", $"{path}{newName}");
					if (File.Exists($"{path}{oldName}.meta")) {
						File.Move($"{path}{oldName}.meta", $"{path}{newName}.meta");
					}
					return true;
				} catch (System.Exception) {
					return false;
				}
			}
		}

		private void OnFocus() {
			
		}

		private void OnLostFocus() {
			
		}
	}
}
