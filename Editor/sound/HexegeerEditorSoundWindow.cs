using System.Collections.Generic;
using System.IO;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorSoundWindow : EditorWindow {
		public enum Tab {
			Music,
			SE,
			SystemSE,
		}

		private Tab _currentTab = Tab.Music;
		private ScrollPane _detailView = null;

		private AddressableListPopupBuilder _musicPopupBuilder;

		private List<bool> _descriptionVisibility = new List<bool>();

		private void OnEnable() {
			_musicPopupBuilder = new AddressableListPopupBuilder(type: typeof(AudioClip), rootPath: "music");

			titleContent = new GUIContent("Sound");
			rootVisualElement.Add(CreateView());
		}

		private VisualElement CreateView() {
			TwoPaneSplitView mainView = new TwoPaneSplitView(0, 200f, TwoPaneSplitViewOrientation.Horizontal);

			SelectableEnumList<Tab> listView = new SelectableEnumList<Tab>();
			listView.selectionChanged += (selection) => {
				_currentTab = selection;
				switch(_currentTab) {
					case Tab.Music: {
						SetMusicView(_detailView);
						break;
					}
					case Tab.SE: {
						SetSEView(_detailView);
						break;
					}
					case Tab.SystemSE: {
						SetSystemSEView(_detailView);
						break;
					}
				}
			};
			mainView.Add(listView);


			_detailView = new ScrollPane()
				.Padding(horizontal:24f, vertical:12f);

			mainView.Add(_detailView);

			listView.Select(_currentTab);

			return mainView;
		}

		private void SetMusicView(ScrollPane pane) {
			pane.Clear();

			Row title = new Row();
			ClickButton generateScriptButton = ClickButton.Create()
				.Label("Generate Script");
			generateScriptButton.OnClicked += () => {
				MusicScriptGenerator generator = new MusicScriptGenerator();
				if (generator.Validation(out List<string> messages)) {
					generator.Generate($"sound{Path.DirectorySeparatorChar}MusicId.cs");
				} else {
					EditorUtility.DisplayDialog("Error", string.Join(System.Environment.NewLine, messages), "Ok");
				}
			};
			ClickButton generateResourceButton = ClickButton.Create()
				.Label("Generate Resource");
			generateResourceButton.OnClicked += () => {
				MusicScriptGenerator validation = new MusicScriptGenerator();
				if (validation.Validation(out List<string> messages)) {
					MusicTableGenerator generator = new MusicTableGenerator();
					generator.Generate("MusicTable.asset");
				} else {
					EditorUtility.DisplayDialog("Error", string.Join(System.Environment.NewLine, messages), "Ok");
				}
			};

			title.AddChildren(
				Text.H2("Music"),
				new Spacer().Weight(1f),
				generateScriptButton,
				new Spacer(width:12f),
				generateResourceButton
			);

			pane.Add(title);

			MusicSettings settings = MusicSettings.instance;

			for (int i = 0; i < settings.MusicList.Count; ++i) {
				if (_descriptionVisibility.Count <= i) {
					_descriptionVisibility.Add(false);
				}

				int index = i;
				Row row = new Row();
				Text descriptionButton = Text.Body("〇");
				descriptionButton.RegisterCallback<MouseDownEvent>(evt => {
					if (evt.button == 0) {
						_descriptionVisibility[index] = !_descriptionVisibility[index];
						SetMusicView(pane);
					}
				});

				TextField nameField = new TextField();
				nameField.style.flexBasis = 0f;
				nameField.style.flexGrow = 1f;
				nameField.SetValueWithoutNotify(settings.MusicList[i].name);
				nameField.RegisterValueChangedCallback(v => {
					settings.MusicList[index].name = v.newValue;
					settings.UpdateParameter(index, settings.MusicList[index]);
				});
				
				PopupField<string> assetPopup = _musicPopupBuilder.Generate(settings.MusicList[i].address);
				assetPopup.style.flexBasis = 0f;
				assetPopup.style.flexGrow = 1f;
				assetPopup.RegisterValueChangedCallback(v => {
					settings.MusicList[index].address = v.newValue;
					settings.UpdateParameter(index, settings.MusicList[index]);
				});

				ClickButton upButton = ClickButton.Create()
					.Label("↑");
				upButton.enabledSelf = index > 0;
				upButton.OnClicked += () => {
					bool temp = _descriptionVisibility[index];
					_descriptionVisibility[index] = _descriptionVisibility[index-1];
					_descriptionVisibility[index-1] = temp;

					settings.MoveUp(index);
					SetMusicView(pane);
				};
				ClickButton downButton = ClickButton.Create()
					.Label("↓");
				downButton.enabledSelf = index < settings.MusicList.Count-1;
				downButton.OnClicked += () => {
					bool temp = _descriptionVisibility[index];
					_descriptionVisibility[index] = _descriptionVisibility[index+1];
					_descriptionVisibility[index+1] = temp;

					settings.MoveDown(index);
					SetMusicView(pane);
				};

				ClickButton deleteButton = ClickButton.Create()
					.Label("×");
				deleteButton.OnClicked += () => {
					_descriptionVisibility.RemoveAt(index);
					settings.RemoveAt(index);
					SetMusicView(pane);
				};

				row.AddChildren(
					descriptionButton,
					nameField,
					assetPopup,
					new Spacer(width: 24f),
					upButton,
					downButton,
					new Spacer(width: 24f),
					deleteButton
				);
				pane.Add(row);

				Text descriptionText = Text.Body("description")
					.FontSize(10f)
					.Bold();
				descriptionText.style.display = _descriptionVisibility[i] ? DisplayStyle.Flex : DisplayStyle.None;

				TextField descriptionField = new TextField();
				descriptionField.multiline = true;
				descriptionField.maxLength = 200;
				descriptionField.isDelayed = true;
				descriptionField.SetValueWithoutNotify(settings.MusicList[i].description);
				descriptionField.RegisterValueChangedCallback(v => {
					settings.MusicList[index].description = v.newValue;
					settings.UpdateParameter(index, settings.MusicList[index]);
				});
				descriptionField.style.width = new Length(90, LengthUnit.Percent);
				descriptionField.style.minHeight = 60f;
				descriptionField.style.display = _descriptionVisibility[i] ? DisplayStyle.Flex : DisplayStyle.None;
				descriptionField.style.marginBottom = 12f;
				pane.AddChildren(descriptionText, descriptionField);
			}

			pane.Add(new Spacer(height: 20f));

			ClickButton addButton = ClickButton.Create()
				.Label("+");
			addButton.OnClicked += () => {
				_descriptionVisibility.Add(false);

				settings.Add();
				SetMusicView(pane);
			};
			pane.Add(addButton);
		}
		private void SetSEView(ScrollPane pane) {
			pane.Clear();
			pane.Add(Text.H2("SE"));
		}
		private void SetSystemSEView(ScrollPane pane) {
			pane.Clear();
			pane.Add(Text.H2("SystemSE"));
		}
	}
}