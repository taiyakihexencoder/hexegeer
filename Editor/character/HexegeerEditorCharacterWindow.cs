using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorCharacterWindow : EditorWindow {
		private LayerSettings _layerSettings;
		private ListPopupBuilder<int> _layerPopupBuilder;

		private CharacterColliderSettings _colliderSettings;
		private ListPopupBuilder<int> _physicsColliderPopupBuilder;

		private SelectableList<CharacterSettings.CharacterData> _listView;

		private event System.Action _updateListView;
		private event System.Action _updateDetailView;

		private void OnEnable() {
			_layerSettings = LayerSettings.instance;
			_layerPopupBuilder = _layerSettings.CreateListPopupBuilder();

			_colliderSettings = CharacterColliderSettings.instance;
			_physicsColliderPopupBuilder = _colliderSettings.CreateListPopupBuilder();

			_listView = new SelectableList<CharacterSettings.CharacterData>();
			_listView.selectionChanged += (selection) => {
				_updateDetailView?.Invoke();
			};
			_updateListView += () => {
				_listView.ClearElements();
				foreach(CharacterSettings.CharacterData character in CharacterSettings.instance.Characters) {
					_listView.AddSelection(character, ListItem(character));
				}
			};
			
			titleContent = new GUIContent("Character");
			rootVisualElement.Add(CreateView());
		}

		private void OnFocus() {
			_updateDetailView?.Invoke();
		}

		private VisualElement CreateView() {
			TwoPaneSplitView mainView = new TwoPaneSplitView(0, 200f, TwoPaneSplitViewOrientation.Horizontal);

			mainView.Add(ListView());

			VisualElement detailedViewPane = new VisualElement();
			detailedViewPane.Add(DetailedView());
			mainView.Add(detailedViewPane);

			_updateDetailView += () => {
				detailedViewPane.Clear();
				detailedViewPane.Add(DetailedView());
			};

			return mainView;
		}

		private VisualElement ListView() {
			CharacterSettings settings = CharacterSettings.instance;
			internallib.Column column = new internallib.Column();

			_updateListView?.Invoke();

			Text controlHeader = Text.H3("Control");

			ClickButton generateScriptButton = ClickButton.Create(Align.FlexStart)
				.Label("Generate Script")
				.Margin(vertical:4f, horizontal: 8f);

			ClickButton generateResourceButton = ClickButton.Create(Align.FlexStart)
				.Label("Generate Resource")
				.Margin(vertical:4f, horizontal: 8f);

			generateResourceButton.OnClicked += () => {
				CharacterTableGenerator generator = new CharacterTableGenerator();
				generator.Generate("CharacterTable.asset");
			};


			Text listHeader = Text.H3("Characters");

			ClickButton addButton = ClickButton.Create(Align.FlexEnd)
				.Label("+")
				.Margin(vertical: 12f, horizontal: 8f);
			addButton.OnClicked += () => {
				settings.Add("New Character");
				_updateListView?.Invoke();
				_listView.Select(settings.Characters[settings.Characters.Length-1]);
			};

			column.AddChildren(
				controlHeader,
				generateScriptButton,
				generateResourceButton,
				new Spacer(height:24f),
				listHeader,
				_listView,
				addButton
			);

			return column;
		}

		private VisualElement ListItem(CharacterSettings.CharacterData character) {
			internallib.Column item = new internallib.Column()
				.HorizontalAlignment(Align.Center)
				.Padding(vertical: 8f, horizontal: 12f);

			Text label = Text.Body(character.name)
				.TextAlign(TextAnchor.UpperCenter);

			item.AddChildren(label);


			return item;
		}

		private VisualElement DetailedView() {
			ContentKeySetting.Key[] contentKeys = ContentKeySetting.instance.Keys;
			CharacterSettings settings = CharacterSettings.instance;
			CharacterSettings.CharacterData character = _listView.Selected;

			ScrollPane pane = new ScrollPane()
				.Padding(horizontal: 24f);

			pane.Add(new Spacer(height:24f));

			if (character != null) {
				Text characterHeader = Text.H3("Character Information")
					.Padding(bottom: 8f);
				pane.Add(characterHeader);

				// Character Name
				Row nameRow = new Row();
				Text nameLabel = Text.Body("Name")
					.Weight(1f);
				TextField nameField = new TextField();
				nameField.isDelayed = true;
				nameField.style.flexBasis = 0f;
				nameField.style.flexGrow = 3f;
				nameField.SetValueWithoutNotify(character.name);
				nameField.RegisterValueChangedCallback(v => {
					CharacterSettings.instance.SetName(character, v.newValue);
					_updateListView?.Invoke();
					_listView.Select(character);
				});
				nameRow.AddChildren(nameLabel, new Spacer(width: 10f), nameField);
				pane.Add(nameRow);

				// Layer
				Row layerRow = new Row();
				Text layerLabel = Text.Body("Layer")
					.Weight(1f);
				PopupField<int> layerPopup = _layerPopupBuilder.Generate(character.layer);
				layerPopup.style.flexBasis = 0f;
				layerPopup.style.flexGrow = 3f;
				layerPopup.RegisterValueChangedCallback(v => {
					CharacterSettings.instance.SetLayer(character, v.newValue);
				});

				layerRow.AddChildren(layerLabel, new Spacer(width: 10f), layerPopup);
				pane.Add(layerRow);

				// Observation Point
				Row observationRow = new Row();
				Text observationLabel = Text.Body("Has Observation Point");
				Toggle observationToggle = new Toggle();
				observationToggle.SetValueWithoutNotify(settings.IsObservationPoint(character));
				observationToggle.RegisterValueChangedCallback(v => {
					if (v.newValue) {
						settings.AddObservationPoint(character.id);
					} else {
						settings.RemoveObservationPoint(character.id);
					}
				});
				observationRow.AddChildren(observationLabel, new Spacer().Weight(1f), observationToggle, new Spacer().Weight(2f));
				pane.Add(observationRow);

				pane.Add(new Spacer(height: 12f));

				// Collider
				VisualElement colliderView = new VisualElement();
				SetColliderItem(colliderView, character);
				pane.Add(colliderView);

				pane.Add(new Spacer(height: 24f));

				// Content Keys

				Text contentHeader = Text.H3("Content Group")
					.Padding(bottom: 8f);

				pane.Add(contentHeader);

				List<int> selectedContentKeys = new List<int>(character.contentKeys);

				{
					Row checkbox = new Row();
					Toggle toggle = new Toggle();
					toggle.SetValueWithoutNotify(selectedContentKeys.Contains(ContentKey.Global.value));
					toggle.RegisterValueChangedCallback(v => {
						if (v.newValue) {
							settings.AddContentKeys(character, ContentKey.Global.value);
						} else {
							settings.RemoveContentKey(character, ContentKey.Global.value);
						}
					});
					Text label = Text.Body(nameof(ContentKey.Global));
					checkbox.AddChildren(toggle, label);
					pane.Add(checkbox);
				}

				internallib.Column contentList = new internallib.Column();
				Row contentRow = new Row();
				pane.Add(contentRow);
				for (int i = 0; i < contentKeys.Length; ++i) {
					int contentKey = contentKeys[i].id;
					Row checkbox = new Row()
						.Weight(1f);

					Toggle toggle = new Toggle();
					toggle.SetValueWithoutNotify(selectedContentKeys.Contains(contentKey));
					toggle.RegisterValueChangedCallback(v => {
						if (v.newValue) {
							settings.AddContentKeys(character, contentKey);
						} else {
							settings.RemoveContentKey(character, contentKey);
						}
					});

					Text label = Text.Body(contentKeys[i].name);
					checkbox.AddChildren(toggle, label);

					contentRow.Add(checkbox);
					if (i % 3 == 2) {
						contentRow = new Row();
						pane.Add(contentRow);
					}
				}
				for (int i = (contentKeys.Length+2) % 3; i < 2; ++i) {
					VisualElement ve = new VisualElement();
					ve.style.flexGrow = 1f;
					ve.style.flexBasis = 0f;
					contentRow.Add(ve);
				}

				pane.Add(new Spacer(height: 24f));

				ClickButton deleteButton = ClickButton.Create()
					.Label("Delete");
				deleteButton.OnClicked += DeleteDialog;

				pane.Add(deleteButton);
			}

			pane.Add(new Spacer(height:24f));

			return pane;
		}

		private void SetColliderItem(VisualElement parent, CharacterSettings.CharacterData character) {
			parent.Clear();

			// 現在のコライダー
			CharacterColliderSettings.PhysicsCollider[] colliders = _colliderSettings.PhysicsColliders;
			CharacterColliderSettings.PhysicsCollider currentCollider = null;
			foreach(CharacterColliderSettings.PhysicsCollider collider in colliders) {
				if (collider.id == character.collider) {
					currentCollider = collider;
					break;
				}
			}

			Row colliderRow = new Row()
				.VerticalAlignment(Align.FlexStart);

			internallib.Column headerColumn = new internallib.Column()
				.Weight(1f);
			Text colliderText = Text.Body("Physics Collider");
			headerColumn.AddChildren(colliderText, new Spacer(height: 1f).Weight(1f));


			internallib.Column colliderColumn = new internallib.Column()
				.Weight(3f);

			Row popupRow = new Row();
			Text popupText = Text.Body("Collider")
				.Width(80.0f);
			PopupField<int> popup = _physicsColliderPopupBuilder.Generate(character.collider);
			popup.style.flexBasis = 0f;
			popup.style.flexGrow = 1f;
			popup.RegisterValueChangedCallback(v => {
				CharacterSettings.instance.SetCollider(character, v.newValue);
				SetColliderItem(parent, character);
			});

			ClickButton addColliderButton = ClickButton.Create()
				.Label("New")
				.Margin(horizontal: 12f);
			addColliderButton.OnClicked += () => {
				int id = _colliderSettings.Add();
				CharacterSettings.instance.SetCollider(character, id);
				_colliderSettings.UpdateKeys(_physicsColliderPopupBuilder);
				SetColliderItem(parent, character);
			};

			popupRow.AddChildren(popupText, popup, addColliderButton);
			colliderColumn.Add(popupRow);

			internallib.Column editColumn = new internallib.Column()
				.Padding(left: 32f);
			editColumn.enabledSelf = currentCollider != null;
			editColumn.style.display = currentCollider != null ? DisplayStyle.Flex : DisplayStyle.None;

			float labelWidth = 80f;
			Row nameRow = new Row();
			Text nameLabel = Text.Body("Name")
				.Width(labelWidth);
			TextField nameField = new TextField();
			nameField.SetValueWithoutNotify(currentCollider?.name ?? "");
			nameField.style.flexBasis = 0f;
			nameField.style.flexGrow = 1f;
			nameField.isDelayed = true;
			nameField.RegisterValueChangedCallback(v => {
				if (currentCollider != null) {
					currentCollider.name = v.newValue;
					_colliderSettings.UpdateCollider(currentCollider);
					SetColliderItem(parent, character);
				}
			});
			nameRow.AddChildren(nameLabel, nameField);

			Row radiusRow = new Row();
			Text radiusLabel = Text.Body("Radius")
				.Width(labelWidth);
			FloatField radiusField = new FloatField();
			radiusField.SetValueWithoutNotify(currentCollider?.radius ?? 0f);
			radiusField.style.flexBasis = 0f;
			radiusField.style.flexGrow = 1f;
			radiusField.isDelayed = true;
			radiusField.RegisterValueChangedCallback(v => {
				if (currentCollider != null) {
					currentCollider.radius = v.newValue;
					_colliderSettings.UpdateCollider(currentCollider);
				}
			});
			radiusRow.AddChildren(radiusLabel, radiusField);

			Row heightRow = new Row();
			Text heightLabel = Text.Body("Height")
				.Width(labelWidth);
			FloatField heightField = new FloatField();
			heightField.SetValueWithoutNotify(currentCollider?.height ?? 0f);
			heightField.style.flexBasis = 0f;
			heightField.style.flexGrow = 1f;
			heightField.isDelayed = true;
			heightField.RegisterValueChangedCallback(v => {
				if (currentCollider != null) {
					currentCollider.height = v.newValue;
					_colliderSettings.UpdateCollider(currentCollider);
				}
			});
			heightRow.AddChildren(heightLabel, heightField);

			ClickButton deleteButton = ClickButton.Create(Align.FlexEnd)
				.Label("Delete Collider")
				.Margin(8f);
			deleteButton.OnClicked += () => {
				if (currentCollider != null && currentCollider?.id != 0) {
					if (EditorUtility.DisplayDialog("Confirm", "Delete Collider ?", "Yes", "No")) {
						CharacterSettings.instance.SetCollider(character, 0);

						_colliderSettings.Remove(currentCollider.id);
						_colliderSettings.UpdateKeys(_physicsColliderPopupBuilder);
						SetColliderItem(parent, character);
					}
				}
			};

			editColumn.AddChildren(nameRow, radiusRow, heightRow, deleteButton);

			colliderColumn.AddChildren(editColumn);

			colliderRow.AddChildren(headerColumn, colliderColumn);

			parent.Add(colliderRow);
		}

		private void DeleteDialog() {
			CharacterSettings.CharacterData selected = _listView.Selected;
			if (selected != null) {
				if (EditorUtility.DisplayDialog("Confirm", "Delete this item ?", "Yes", "No")) {
					CharacterSettings settings = CharacterSettings.instance;
					settings.Remove(selected.id);
					_updateListView?.Invoke();
				}
			}
		}
	}
}
