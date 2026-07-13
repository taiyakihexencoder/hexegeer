using System.Collections.Generic;
using System.IO;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorFieldWindow : EditorWindow {
		private ListPopupBuilder<int> popupBuilder;

		private void OnEnable() {
			titleContent = new GUIContent("Field");

			ContentKeySetting keySettings = ContentKeySetting.instance;

			popupBuilder = keySettings.CreateListPopupBuilder();

			VisualElement mainView = CreateMainView();
			rootVisualElement.Add(mainView);
		}

		private void OnFocus() {
			ContentKeySetting keySettings = ContentKeySetting.instance;
			popupBuilder = keySettings.UpdateKeys(popupBuilder);
		}

		private VisualElement CreateMainView() {
			FieldMainSettings mainSettings = FieldMainSettings.instance;

			ScrollPane frame = new ScrollPane()
				.Padding(vertical: 16, horizontal: 24);

			frame.Add(Text.H2("Editor Mode"));

			EnumField field = new EnumField("View Type", mainSettings.ViewType);
			field.RegisterValueChangedCallback(v => {
				mainSettings.ViewType = (FieldViewType) v.newValue;
			});

			frame.Add(field);

			frame.Add(new Spacer(height: 12f));

			// Load Field Distance
			FloatField loadFieldDistanceField = new FloatField("Load Distance");
			loadFieldDistanceField.SetValueWithoutNotify(mainSettings.LoadFieldDistance);
			loadFieldDistanceField.RegisterValueChangedCallback(v => {
				mainSettings.LoadFieldDistance = v.newValue;
			});
			loadFieldDistanceField.isDelayed = true;
			frame.Add(loadFieldDistanceField);
			
			// Unload Field Distance
			FloatField unloadFieldDistanceField = new FloatField("Unload Distance");
			unloadFieldDistanceField.SetValueWithoutNotify(mainSettings.UnloadFieldDistance);
			unloadFieldDistanceField.RegisterValueChangedCallback(v => {
				mainSettings.UnloadFieldDistance = v.newValue;
			});
			unloadFieldDistanceField.isDelayed = true;
			frame.Add(unloadFieldDistanceField);

			// Mesh Count
			IntegerField cacheCountField = new IntegerField("Cache Count");
			cacheCountField.SetValueWithoutNotify(mainSettings.MeshCacheCount);
			cacheCountField.RegisterValueChangedCallback(v => {
				mainSettings.MeshCacheCount = Mathf.Max(0, v.newValue);
			});
			cacheCountField.isDelayed = true;
			frame.Add(cacheCountField);

			// Update Interval
			DoubleField updateIntervalField = new DoubleField("Update Interval");
			updateIntervalField.SetValueWithoutNotify(mainSettings.UpdateInterval);
			updateIntervalField.RegisterValueChangedCallback(v => {
				mainSettings.UpdateInterval = v.newValue < 0.0 ? 0.0 : v.newValue;
			});
			updateIntervalField.isDelayed = true;
			frame.Add(updateIntervalField);

			frame.Add(new Spacer(height: 20f));

			// Each View Settings
			switch (mainSettings.ViewType) {
				case FieldViewType.SideView: {
					frame.Add(SideViewSettingsView());
					break;
				}
			}

			frame.Add(new Spacer(height:20f));
			
			frame.Add(FieldAssetListView());

			frame.Add(new Spacer(height:20f));

			ClickButton resourceButton = ClickButton.Create()
				.Label("Generate Runtime Resource");
			resourceButton.OnClicked += OnRequestGenerateResource;

			ClickButton scriptButton = ClickButton.Create()
				.Label("Generate Script");
			scriptButton.OnClicked += OnRequestGenerateScript;

			Row buttons = new Row();
			buttons.AddChildren(resourceButton, new Spacer(width: 24f), scriptButton);


			frame.Add(buttons);

			return frame;
		}

		private VisualElement SideViewSettingsView() {
			FieldSideViewSettings settings = FieldSideViewSettings.instance;

			internallib.Column column = new internallib.Column()
				.Padding(vertical: 12);

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

		private VisualElement FieldAssetListView() {
			FieldMainSettings mainSettings = FieldMainSettings.instance;

			internallib.Column column = new internallib.Column();

			column.AddChildren(Text.H2("Field Assets"), new Spacer(height: 12f));

			// ヘッダ
			Row header = new Row()
				.Height(25f)
				.Align(Align.FlexStart)
				.Background(Color.gray)
				.Border(Color.white);

			Text headerGuid = Text.Body("Guid").Width(200.0f).TextColor(Color.black);
			Text headerAssetPath = Text.Body("Location").Weight(1f).TextColor(Color.black);
			Text headerKey = Text.Body("Key").Width(100f).TextColor(Color.black);
			Text headerAddress = Text.Body("Address").Weight(1f).TextColor(Color.black);
			header.AddChildren(headerGuid, headerAssetPath, headerKey, headerAddress);

			column.Add(header);

			// コンテンツリスト
			System.Type resourceType = mainSettings.ViewType.GetResourceType();
			string[] guids = AssetDatabase.FindAssets($"t:{resourceType.Name}");

			List<BaseFieldBlueprint> notRegisteredList = new List<BaseFieldBlueprint>();
			List<BaseFieldBlueprint> registeredList = new List<BaseFieldBlueprint>();
			List<int> idList = new List<int>();
			Dictionary<string, BaseFieldBlueprint> table = new Dictionary<string, BaseFieldBlueprint>();
			foreach(string guid in guids) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				BaseFieldBlueprint blueprint = AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath);

				// まだ初期化されていないアセット
				if (blueprint.Id <= 0) {
					notRegisteredList.Add(blueprint);
				} else {
					registeredList.Add(blueprint);
					idList.Add(blueprint.Id);
				}

				table.Add(guid, blueprint);
			}

			idList.Sort();

			// 初期化されていなければIDを振る
			int currentId = 1;
			int i = 0;
			foreach(BaseFieldBlueprint blueprint in notRegisteredList) {
				for (; i < registeredList.Count; ++i) {
					if (idList[i] - currentId > 0) {
						idList.Insert(i, currentId);
						break;
					}
				}
				SerializedObject obj = new SerializedObject(blueprint);
				obj.FindProperty("_id").intValue = currentId;
				obj.ApplyModifiedProperties();
				currentId++;
				++i;
			}

			foreach(string guid in table.Keys) {
				Row row = new Row()
					.Height(20f)
					.VerticalAlignment(Align.Center)
					.Border(Color.white);

				Text guidLabel = Text.Body(guid)
					.TextAlign(TextAnchor.MiddleLeft)
					.Width(200f);
				guidLabel.style.fontSize = 10f;

				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				Text assetPathLabel = Text.Body(assetPath)
					.Weight(1f);
				assetPathLabel.style.fontSize = 10f;

				BaseFieldBlueprint blueprint = table[guid];

				PopupField<int> contentKeyPopup = popupBuilder.Generate(blueprint.ContentKey);
				contentKeyPopup.style.width = 100f;
				contentKeyPopup.RegisterValueChangedCallback(v => {
					SerializedObject obj = new SerializedObject(blueprint);
					obj.FindProperty("_contentKey").intValue = v.newValue;
					obj.ApplyModifiedProperties();
				});

				TextField runtimeAssetAddressField = new TextField("");
				runtimeAssetAddressField.style.flexBasis = 0f;
				runtimeAssetAddressField.style.flexGrow = 1f;
				runtimeAssetAddressField.style.fontSize = 10f;

				runtimeAssetAddressField.RegisterValueChangedCallback(v => {
					SerializedObject obj = new SerializedObject(blueprint);
					obj.FindProperty("_runtimeAssetAddress").stringValue = v.newValue;
					obj.ApplyModifiedProperties();
				});
				string runtimeAssetAddress = table[guid].RuntimeAssetAddress;

				// 設定されていないなら設定しておくしCallbackでアセットも更新する
				if (string.IsNullOrEmpty(runtimeAssetAddress)) {
					runtimeAssetAddressField.schedule.Execute (() =>{
						string fileName = table[guid].name;
						runtimeAssetAddressField.value = $"hexegeer/field/{fileName}";
						EditorApplication.delayCall += () => {
						};
					}).ExecuteLater(100);
				} else {
					runtimeAssetAddressField.SetValueWithoutNotify(runtimeAssetAddress);
				}

				row.AddChildren(guidLabel, assetPathLabel, contentKeyPopup, runtimeAssetAddressField);
				column.Add(row);
			}

			return column;
		}

		private void OnRequestGenerateResource() {
			FieldMainSettings settings = FieldMainSettings.instance;
			System.Type resourceType = settings.ViewType.GetResourceType();
			foreach (string guid in AssetDatabase.FindAssets($"t:{resourceType.Name}")) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				BaseFieldBlueprint blueprint = AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath);

				ResourceGenerator<FieldMeshResource> generator = new FieldResourceGenerator(blueprint);
				generator.Generate(blueprint.name + ".asset");
			}

			ResourceGenerator<FieldTable> tableGenerator = new FieldTableGenerator();
			tableGenerator.Generate($"{typeof(FieldTable).Name}.asset");
		}

		private void OnRequestGenerateScript() {
			FieldScriptGenerator generator = new FieldScriptGenerator();
			if (generator.Validation(out List<string> messages)) {
				generator.Generate($"field{Path.DirectorySeparatorChar}FieldAutoGenerates.cs");
			} else {
				EditorUtility.DisplayDialog(
					title: "Error",
					message: string.Join('\n', messages),
					ok: "ok"
				);
			}
		}
	}
}