using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorDamageObjectWindow : EditorWindow {

		private ListPopupBuilder<int> _layerPopupBuilder;
		private ListPopupBuilder<int> _colliderPopupBuilder;

		private ScrollPane _pane;
		private int _selectedIndex;

		private void OnEnable() {
			_pane = new ScrollPane()
				.Padding(horizontal:24f);

			_layerPopupBuilder = LayerSettings.instance.CreateListPopupBuilder();

			_colliderPopupBuilder = DamageObjectColliderSettings.instance.CreateListPopupBuilder();

			titleContent = new GUIContent("Damage Object");

			rootVisualElement.Add(_pane);
		}

		private void OnFocus() {
			_layerPopupBuilder = LayerSettings.instance.UpdateKeys(_layerPopupBuilder);
			_colliderPopupBuilder = DamageObjectColliderSettings.instance.UpdateKeys(_colliderPopupBuilder);
			_selectedIndex = -1;
			UpdatePane();
		}

		private void UpdatePane() {
			_pane.Clear();

			Row titleRow = new Row();
			ClickButton generatorButton = ClickButton.Create()
				.Label("Generate Resource");
			generatorButton.OnClicked += () => {
				DamageObjectTableGenerator generator = new DamageObjectTableGenerator();
				generator.Generate("DamageObjectTable.asset");
			};

			titleRow.AddChildren(Text.H2("Damage Object"), new Spacer(width: 64f), generatorButton);

			_pane.AddChildren(new Spacer(height: 16f), titleRow);

			List<DamageObjectSettings.DamageObjectData> rows = DamageObjectSettings.instance.Rows;
			for (int i = 0; i < rows.Count; ++i) {
				_pane.Add(DamageObjectProfileLayout(i, rows[i]));
			}

			ClickButton addButton = ClickButton.Create()
				.Label("+");
			addButton.OnClicked += () => {
				DamageObjectSettings.instance.AddRow();
				UpdatePane();
			};
			_pane.Add(addButton);

			_pane.Add(new Spacer(20f));
		}

		private VisualElement DamageObjectProfileLayout(int index, DamageObjectSettings.DamageObjectData data) {
			Text header = Text.Body(data.name);
			internallib.Foldout foldout = new internallib.Foldout(header, _selectedIndex == index)
				.Margin(8f);
			foldout.onExpandedStateChanged += (expanded) => {
				_selectedIndex = expanded ? index : -1;
				UpdatePane();
			};

			internallib.Column column = new internallib.Column()
				.Padding(horizontal:24f, vertical:8f);
			
			Row nameRow = new Row();

			TextField nameField = new TextField();
			nameField.style.flexBasis = 0f;
			nameField.style.flexGrow = 1f;
			nameField.SetValueWithoutNotify(data.name);
			nameField.RegisterValueChangedCallback(v => {
				data.name = v.newValue;
				header.text = v.newValue;
				DamageObjectSettings.instance.UpdateRow(index, data);
			});

			ClickButton removeButton = ClickButton.Create()
				.Label("-");
			removeButton.OnClicked += () => {
				_selectedIndex = -1;
				DamageObjectSettings.instance.RemoveRow(index);
				UpdatePane();
			};


			nameRow.AddChildren(
				Text.Body("Name"), 
				nameField, 
				new Spacer(width: 24f),
				removeButton
			);

			Row layerRow = new Row();
			PopupField<int> layerPopup = _layerPopupBuilder.Generate(data.layer);
			layerPopup.style.flexBasis = 0f;
			layerPopup.style.flexGrow = 1f;
			layerPopup.RegisterValueChangedCallback(v => {
				data.layer = v.newValue;
				DamageObjectSettings.instance.UpdateRow(index, data);
			});

			layerRow.AddChildren(Text.Body("Layer"), new Spacer(width:8f), layerPopup, new Spacer().Weight(1f));


			Row contentKeyRow = new Row();
			List<ContentKeySetting.Key> contentKeys = ContentKeySetting.instance.Keys;
			Dictionary<ContentKeySetting.Key, bool> keySelectList = new Dictionary<ContentKeySetting.Key, bool>();
			keySelectList.Add(new ContentKeySetting.Key{ id = ContentKey.Global.value, name = nameof(ContentKey.Global)}, data.contentKeys.Contains(ContentKey.Global.value));
			foreach(ContentKeySetting.Key key in contentKeys) {
				keySelectList.Add(key, data.contentKeys.Contains(key.id));
			}
			MultiSelectDropdown<ContentKeySetting.Key> contentKeyDropdown = new MultiSelectDropdown<ContentKeySetting.Key>()
				.Weight(1f)
				.SetKeys(keySelectList, key => key.name);
			contentKeyDropdown.style.maxWidth = 400f;
			contentKeyDropdown.OnSelectionChanged += (key, load) => {
				if (load) {
					data.contentKeys.Add(key.id);
				} else {
					data.contentKeys.Remove(key.id);
				}
			};

			contentKeyRow.AddChildren(
				Text.Body("Content Keys"),
				contentKeyDropdown,
				new Spacer(width: 100f)
			);

			column.AddChildren(
				nameRow, 
				new Spacer(height: 8f),
				layerRow,
				new Spacer(height: 8f),
				contentKeyRow, 
				new Spacer(height: 8f),
				ColliderLayout(index, data)
			);
			foldout.Add(column);
			return foldout;
		}

		private VisualElement ColliderLayout(int index, in DamageObjectSettings.DamageObjectData data) {
			internallib.Column column = new internallib.Column();
			ColliderInternalLayout(column, index, data);
			
			return column;
		}

		private void ColliderInternalLayout(internallib.Column column, int index, in DamageObjectSettings.DamageObjectData data) {
			DamageObjectSettings.DamageObjectData newData = data;
			column.Clear();

			Row selectorRow = new Row();

			PopupField<int> colliderPopup = _colliderPopupBuilder.Generate(data.collider);
			colliderPopup.RegisterValueChangedCallback(v => {
				newData.collider = v.newValue;
				DamageObjectSettings.instance.UpdateRow(index, newData);
				ColliderInternalLayout(column, index, newData);
			});
			colliderPopup.style.flexBasis = 0f;
			colliderPopup.style.flexGrow = 1f;

			ClickButton newButton = ClickButton.Create()
				.Label("New");
			newButton.OnClicked += () => {
				int id = DamageObjectColliderSettings.instance.AddCollider();
				_colliderPopupBuilder = DamageObjectColliderSettings.instance.UpdateKeys(_colliderPopupBuilder);
				newData.collider = id;
				DamageObjectSettings.instance.UpdateRow(index, newData);
				ColliderInternalLayout(column, index, newData);
			};
			selectorRow.AddChildren(Text.Body("Collider"), colliderPopup, new Spacer(width:12f), newButton);

			column.Add(selectorRow);
			DamageObjectColliderSettings.Geometry? geometry = DamageObjectColliderSettings.instance.Get(data.collider);
			if (geometry != null) {
				DamageObjectColliderSettings.Geometry geom = geometry.Value;
				
				internallib.Column innerColumn = new internallib.Column()
					.Margin(left: 32f);
				Text title = Text.Body("Collider Property");
				Row propertyRow = new Row();
				TextField nameField = new TextField();
				nameField.isDelayed = true;
				nameField.SetValueWithoutNotify(geom.name);
				nameField.RegisterValueChangedCallback(v => {
					geom.name = v.newValue;
					DamageObjectColliderSettings.instance.UpdateCollider(geom);
					_colliderPopupBuilder = DamageObjectColliderSettings.instance.UpdateKeys(_colliderPopupBuilder);
				});
				nameField.style.flexBasis = 0f;
				nameField.style.flexGrow = 2f;

				EnumField shapeField = new EnumField(geometry.Value.shape);
				shapeField.style.flexBasis = 0f;
				shapeField.style.flexGrow = 1f;
				shapeField.RegisterValueChangedCallback(v => {
					geom.shape = (DamageObjectColliderSettings.GeometryShape)v.newValue;
					DamageObjectColliderSettings.instance.UpdateCollider(geom);
					ColliderInternalLayout(column, index, newData);
				});
				
				propertyRow.AddChildren(
					Text.Body("Name"), nameField,
					new Spacer(width: 24f),
					Text.Body("Shape"), shapeField
				);

				// extent
				Row extentRow = new Row();
				switch(geom.shape) {
					case DamageObjectColliderSettings.GeometryShape.Sphere: {
						FloatField extentField = new FloatField();
						extentField.style.flexBasis = 0f;
						extentField.style.flexGrow = 1f;
						extentField.SetValueWithoutNotify(geom.extent.x);
						extentField.RegisterValueChangedCallback(v => {
							geom.extent = new Vector3(v.newValue, v.newValue, v.newValue);
							DamageObjectColliderSettings.instance.UpdateCollider(geom);
						});
						extentRow.AddChildren(
							Text.Body("Extent"),
							new Spacer(width: 12f),
							Text.Body("R"), 
							extentField
						);
						break;
					}
					case DamageObjectColliderSettings.GeometryShape.Box: {
						Vector3Field extentField = new Vector3Field();
						extentField.style.flexBasis = 0f;
						extentField.style.flexGrow = 1f;
						extentField.SetValueWithoutNotify(geom.extent);
						extentField.RegisterValueChangedCallback(v => {
							geom.extent = v.newValue;
							DamageObjectColliderSettings.instance.UpdateCollider(geom);
						});
						extentRow.AddChildren(
							Text.Body("Extent"), 
							new Spacer(width: 12f),
							extentField
						);
						break;
					}
					case DamageObjectColliderSettings.GeometryShape.Cylinder: {
						FloatField radiusField = new FloatField();
						radiusField.style.flexBasis = 0f;
						radiusField.style.flexGrow = 1f;
						radiusField.SetValueWithoutNotify(geom.extent.x);
						radiusField.RegisterValueChangedCallback(v => {
							geom.extent = new Vector3(v.newValue, geom.extent.y, v.newValue);
							DamageObjectColliderSettings.instance.UpdateCollider(geom);
						});
						FloatField heightField = new FloatField();
						heightField.style.flexBasis = 0f;
						heightField.style.flexGrow = 1f;
						heightField.SetValueWithoutNotify(geom.extent.y);
						heightField.RegisterValueChangedCallback(v => {
							geom.extent = new Vector3(geom.extent.x, v.newValue, geom.extent.x);
						});
						extentRow.AddChildren(
							Text.Body("Extent"),
							new Spacer(width: 12f),
							Text.Body("R"), radiusField,
							new Spacer(width: 12f),
							Text.Body("H"), heightField
						);
						break;
					}
				}

				ClickButton deleteButton = ClickButton.Create(Align.FlexEnd)
					.Label("Delete Collider")
					.Margin(horizontal: 8f);
				deleteButton.OnClicked += () => {
					if (EditorUtility.DisplayDialog("Confirm", "Delete Collider ?", "Yes", "No")) {
						newData.collider = 0;
						DamageObjectSettings.instance.UpdateRow(index, newData);

						DamageObjectColliderSettings.instance.RemoveCollider(geom.id);
						_colliderPopupBuilder = DamageObjectColliderSettings.instance.UpdateKeys(_colliderPopupBuilder);
						ColliderInternalLayout(column, index, newData);
					}
				};

				innerColumn.AddChildren(
					title, 
					new Spacer(height: 8f),
					propertyRow, 
					new Spacer(height: 8f),
					extentRow, 
					new Spacer(height: 8f),
					deleteButton
				);
				column.Add(innerColumn);
			}
		}
	}
}