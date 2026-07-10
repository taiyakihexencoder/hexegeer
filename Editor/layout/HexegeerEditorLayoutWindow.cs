using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorLayoutWindow : PreviewWindow {
		private const int CAPSULE_RESOLUTION = 4;

		protected override Rect PreviewRect => new Rect(
			new Vector2(5f, 5f),
			position.size - new Vector2(10f, 10f)
		);

		private ListPopupBuilder<int> _contentKeyPopupBuilder;
		private internallib.Column _detailView;

		private bool _openDetail;
		private bool _initialized;

		private List<BaseFieldBlueprint> _blueprints = new List<BaseFieldBlueprint>();
		private List<IViewContents> _viewContents = new List<IViewContents>();

		private Color _characterColor = new Color(0.0f, 1.0f, 1.0f, 0.5f);
		private Color _noColliderColor = new Color(1.0f, 0.0f, 0.25f, 0.5f);
		private Material _characterMaterial;
		private Material _noColliderCharacterMaterial;

		private int _selectedContentKey = ContentKey.Global.value;

		protected override void OnEnablePreview() {
			titleContent = new GUIContent("Layout");

			_initialized = false;

			_contentKeyPopupBuilder = ContentKeySetting.instance.CreateListPopupBuilder();
			_detailView = new internallib.Column();

			VisualElement rootView = CreateView();
			rootView.pickingMode = PickingMode.Ignore;

			rootVisualElement.Add(rootView);
			rootView.StretchToParentSize();
		}

		private void OnFocus() {
			_openDetail = false;
			
			if (!_initialized) {
				_initialized = true;
				camera.fov = 60f;

				FieldMainSettings fieldSettings = FieldMainSettings.instance;
				System.Type type = fieldSettings.ViewType.GetResourceType();
				foreach(string guid in AssetDatabase.FindAssets($"t:{type.Name}")) {
					string assetPath = AssetDatabase.GUIDToAssetPath(guid);
					_blueprints.Add(AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath));
				}
			}
			OnSelectContentKey(_selectedContentKey);

			// ContentKeyの変更を反映する
			ContentKeySetting contentKeySetting = ContentKeySetting.instance;
			LayoutSetting layoutSetting = LayoutSetting.instance;
			layoutSetting.UpdateLayouts(contentKeySetting.Keys.Map(_ => _.id));
			_contentKeyPopupBuilder = ContentKeySetting.instance.UpdateKeys(_contentKeyPopupBuilder);

			previewPaused = false;
		}

		private void OnLostFocus() {
			_detailView.Clear();
			foreach(IViewContents content in _viewContents) {
				content.OnDestroy();
			}
			_viewContents.Clear();
			previewPaused = true;
		}

		private void OnDestroy() {
			if (_characterMaterial != null) {
				DestroyImmediate(_characterMaterial);
				_characterMaterial = null;
			}

			if (_noColliderCharacterMaterial != null) { 
				DestroyImmediate(_noColliderCharacterMaterial);
				_noColliderCharacterMaterial = null;
			}
		}

		private VisualElement CreateView() {
			VisualElement layeredView = new VisualElement();

			VisualElement overlay = new VisualElement();
			OverlayLayout(overlay);
			layeredView.Add(overlay);

			overlay.StretchToParentSize();

			return layeredView;
		}

		private void OverlayLayout(VisualElement overlayView) {
			overlayView.Clear();
			// 重なっている下のレイヤーがクリックに反応するようにする
			overlayView.pickingMode = PickingMode.Ignore;

			ContentKeySetting contentKeySettings = ContentKeySetting.instance;

			// Content Key Popup
			{
				VisualElement popupElement = new VisualElement();
				popupElement.style.flexDirection = FlexDirection.Row;
				popupElement.style.position = Position.Absolute;
				popupElement.style.right = new Length(20f);
				popupElement.style.top = new Length(10f);

				Text popupText = Text.Body("Content Key")
					.TextColor(Color.black);
				popupElement.Add(popupText);

				PopupField<int> contentKeyPopup = _contentKeyPopupBuilder.Generate(_selectedContentKey);
				popupElement.Add(contentKeyPopup);
				contentKeyPopup.RegisterValueChangedCallback(v => {
					OnSelectContentKey(v.newValue);
				});

				overlayView.Add(popupElement);
			}

			// Generate Resource Button
			{
				ClickButton generateButton = ClickButton.Create()
					.Label("Generate Resource");
				generateButton.style.position = Position.Absolute;
				generateButton.style.right = new Length(20f);
				generateButton.style.bottom = new Length(10f);
				generateButton.OnClicked += () => {
					LayoutResourceGenerator generator = new LayoutResourceGenerator();
					generator.Generate("LayoutTable.asset");
				};
				overlayView.Add(generateButton);
			}


			// detail view
			if (_openDetail) {
				// item list
				{
					ScrollPane listPane = new ScrollPane()
						.Background(new Color(0.0f, 0.0f, 0.0f, 0.8f))
						.Border(Color.white, 1f, 12f)
						.Padding(horizontal: 8f);
					listPane.style.height = new Length(98f, LengthUnit.Percent);
					listPane.style.position = Position.Absolute;
					listPane.style.left = 10f;
					listPane.style.width = new Length(30f, LengthUnit.Percent);
					listPane.style.top = new Length(1f, LengthUnit.Percent);
					listPane.style.bottom = new Length(1f, LengthUnit.Percent);
					listPane.style.opacity = 0.75f;

					listPane.AddChildren(new Spacer(height: 12f), _detailView, new Spacer(height: 50f));

					overlayView.Add(listPane);
				}

				// close button
				{
					ClickButton button = ClickButton.Create()
						.Label("-")
						.Circle();
					button.OnClicked += () => {
						_openDetail = false;
						OverlayLayout(overlayView);
					};
					button.style.position = Position.Absolute;
					button.style.left = new Length(20f);
					button.style.bottom = new Length(20f);
					overlayView.Add(button);
				}
			} else {
				// open button
				ClickButton button = ClickButton.Create()
					.Label("+")
					.Circle();
				button.OnClicked += () => {
					_openDetail = true;
					OverlayLayout(overlayView);
				};
				button.style.position = Position.Absolute;
				button.style.left = new Length(20f);
				button.style.bottom = new Length(20f);
				overlayView.Add(button);
			}
		}

		private void OnSelectContentKey(int contentKey) {
			_detailView.Clear();
			foreach(IViewContents content in _viewContents) {
				content.OnDestroy();
			}
			_viewContents.Clear();

			InitMaterial(ref _characterMaterial, _characterColor);
			InitMaterial(ref _noColliderCharacterMaterial, _noColliderColor);

			_selectedContentKey = contentKey;

			if (contentKey != ContentKey.Global.value) {
				LoadContentKey(_selectedContentKey);
			}
		}

		private void InitMaterial(ref Material material, Color color) {
			if (material != null) { DestroyImmediate(material); }
			material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
			material.SetFloat("_Surface", 1);
			material.SetFloat("_Blend", 0);
			material.SetInt("_BlendOp", (int)BlendOp.Add);
			material.SetInt("_SrcBlend", (int)BlendMode.One);
			material.SetInt("_DstBlend", (int)BlendMode.One);
			material.SetInt("_SrcBlendAlpha", (int)BlendMode.SrcAlpha);
			material.SetInt("_DstBlendAlpha", (int)BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 0);
			material.renderQueue = (int)RenderQueue.Transparent;
			material.SetColor("_BaseColor", color);
		}

		private void LoadContentKey(int contentKey) {
			// フィールドの読込
			FieldMainSettings mainSettings = FieldMainSettings.instance;
			FieldViewType viewType = mainSettings.ViewType;

			BaseFieldBlueprint bp = _blueprints.Find(_ => _.ContentKey == contentKey);
			if (bp != null) {
				FieldMeshContents field = new FieldMeshContents(bp);

				// 視点の中心を領域の中心に
				Vector3 center = field.BoundsMin + (field.BoundsMax - field.BoundsMin) * 0.5f;
				camera.pivotPosition = center;

				if (viewType == FieldViewType.SideView) {
					// 0度だとメッシュと並行して見えないのでちょっとずらす
					camera.rotation = Quaternion.AngleAxis(20f, Vector3.right);

					// 映すべきxy平面の領域から必要な距離を計算
					Vector3 size = field.BoundsMax - field.BoundsMin;
					Vector2 viewSize = PreviewRect.size;
					float ratio = viewSize.x / viewSize.y;
					float requireViewWidth = Mathf.Max(size.x, size.y * ratio);
					float distance = requireViewWidth * Mathf.Tan(Mathf.Deg2Rad * camera.fov * 0.5f);
					camera.SetZoomRange(1f, distance);
					camera.distance = distance;

					// クリップは余裕を持たせる
					camera.farClipPlane = distance * 1.5f;
				}
				(field as IViewContents).AddTo(scene);
				_viewContents.Add(field);
			}

			LayoutSetting layoutSetting = LayoutSetting.instance;
			int layoutIndex = layoutSetting.LayoutProfiles.FindIndex(_ => _.contentKey == contentKey);


			if (layoutIndex >= 0) { 
				LayoutSetting.LayoutProfile layout = layoutSetting.LayoutProfiles[layoutIndex];
				CreateLayoutDetailView(layoutIndex);
			}
		}

		private void CreateLayoutDetailView(int layoutIndex) {
			_detailView.Clear();

			// 再描画を考慮して作成コンテンツをすべて破棄
			for(int i = _viewContents.Count-1; i >= 0; --i) {
				if (_viewContents[i] is CapsuleViewContents) {
					_viewContents[i].OnDestroy();
					_viewContents.RemoveAt(i);
				}
			}

			LayoutSetting layoutSetting = LayoutSetting.instance;
			LayoutSetting.LayoutProfile layout = layoutSetting.LayoutProfiles[layoutIndex];

			CharacterSettings characterSettings = CharacterSettings.instance;
			CharacterColliderSettings colliderSettings = CharacterColliderSettings.instance;
			List<CharacterSettings.CharacterData> characters = characterSettings.Characters;
			ListPopupBuilder<int> characterPopupBuilder = characterSettings.CreateListPopupBuilder();

			_detailView.Add(Text.H3("Characters"));
			for (int i = 0; i < layout.characters.Count; ++i) {
				_detailView.Add(new Spacer(height: 6f));
				int characterIndex = i;
				LayoutSetting.CharacterLayout character = layout.characters[i];
				CharacterSettings.CharacterData data = characters.Find(_ => _.id == character.character);
				IViewContents sceneContents = AddCharacterContent(character.character, character.position, character.rotation);

				Row headerRow = new Row()
					.HorzontalArrangement(Justify.SpaceBetween)
					.Weight(1f);
				ClickButton deleteButton = ClickButton.Create(Align.FlexEnd)
					.Circle(16f)
					.Label("-");
				deleteButton.OnClicked += () => {
					layoutSetting.RemoveCharacter(layoutIndex, characterIndex);
					CreateLayoutDetailView(layoutIndex);
				};
				Text nameText = Text.Body(data?.name ?? "!Undefined");
				headerRow.AddChildren(nameText, deleteButton);

				internallib.Foldout headerFoldout = new internallib.Foldout(headerRow)
					.Background(new Color(0.2f, 0.2f, 0.2f))
					.Border(Color.white, 1f, 8f)
					.Padding(horizontal: 12f, vertical: 6f);

				PopupField<int> characterPopup = characterPopupBuilder.Generate(character.character);
				characterPopup.RegisterValueChangedCallback(v => {
					layoutSetting.UpdateCharacter(layoutIndex, characterIndex, v.newValue);
					// GameObjectを作り直しなのでビューごと更新
					CreateLayoutDetailView(layoutIndex);
				});

				Text positionText = Text.Body("Position");
				Vector3Field positionField = new Vector3Field();
				positionField.style.paddingLeft = 20f;
				positionField.SetValueWithoutNotify(character.position);
				positionField.RegisterValueChangedCallback(v => {
					layoutSetting.UpdateCharacterPosition(layoutIndex, characterIndex, v.newValue);
					if (sceneContents != null) {
						sceneContents.GameObject.transform.position = v.newValue;
					}
				});

				Text rotationText = Text.Body("Rotation");
				Vector3Field rotationField = new Vector3Field();
				rotationField.style.paddingLeft = 20f;
				rotationField.SetValueWithoutNotify(character.rotation.eulerAngles);
				rotationField.RegisterValueChangedCallback(v => {
					Quaternion rotation = Quaternion.Euler(v.newValue);
					layoutSetting.UpdateCharacterRotation(layoutIndex, characterIndex, rotation);
					if (sceneContents != null) {
						sceneContents.GameObject.transform.rotation = rotation;
					}
				});
				headerFoldout.AddChildren(
					new Spacer(height: 12f),
					characterPopup, 
					positionText, 
					positionField, 
					rotationText, 
					rotationField
				);
				_detailView.Add(headerFoldout);
			}
			ClickButton addCharacterButton = ClickButton.Create(Align.FlexEnd)
				.Label("+")
				.Circle(30f)
				.Margin(12f);
			addCharacterButton.OnClicked += () => {
				layoutSetting.AddCharacter(layoutIndex);
				CreateLayoutDetailView(layoutIndex);
			};
			_detailView.Add(addCharacterButton);

		}

		private IViewContents AddCharacterContent(int characterId, Vector3 position, Quaternion rotation) {
			CharacterSettings characterSettings = CharacterSettings.instance;
			CharacterColliderSettings colliderSettings = CharacterColliderSettings.instance;
			List<CharacterSettings.CharacterData> characters = characterSettings.Characters;

			CharacterSettings.CharacterData data = characters.Find(_ => _.id == characterId);
			CharacterColliderSettings.PhysicsCollider collider = colliderSettings.PhysicsColliders.Find(_ => _.id == data?.id);
			IViewContents sceneContents = null;
			if (collider != null) {
				sceneContents = CapsuleContents(
					data?.name ?? "!Undefined",
					position,
					rotation,
					collider.radius,
					collider.height,
					_characterMaterial
				);
				sceneContents.AddTo(scene);
				_viewContents.Add(sceneContents);
			} else {
				sceneContents = CapsuleContents(
					data?.name ?? "!Undefined",
					position,
					rotation,
					0.5f,
					2f,
					_noColliderCharacterMaterial
				);
				sceneContents.AddTo(scene);
				_viewContents.Add(sceneContents);
			}
			return sceneContents;
		}


		private IViewContents CapsuleContents(
			string name,
			Vector3 position,
			Quaternion rotation,
			float radius, 
			float height,
			Material material
		) {
			return new CapsuleViewContents(
				CreateCapsule(radius, height, CAPSULE_RESOLUTION), 
				material, 
				position, 
				rotation,
				new Vector3(0.0f, height + Mathf.Min(height*0.2f, 0.25f), 0.0f),
				name
			);
		}

		private Mesh CreateCapsule(float radius, float height, int resolution) {
			// 90度の弧を作る頂点数をnとすると、
			// 半球を作るのに必要な頂点は、
			// 1 + (n-1) * (n * 4 - 3)
			int hemiSphereVertexCount = 1 + (resolution-1) * (resolution * 4 - 3);

			Vector3[] vertices = new Vector3[hemiSphereVertexCount * 2];

			float x, y, z, r, rad;
			int circleVertices = resolution * 4 - 3;
			float halfCylinderHeight = height * 0.5f - radius;
			for (int sign = -1, k = 0; sign <= 1; sign += 2, k += hemiSphereVertexCount) {
				for (int i = 0, offset; i < resolution-1; ++i) {
					y = halfCylinderHeight + radius + sign * (radius * Mathf.Sin(i * Mathf.PI / (2 * (resolution-1))) + halfCylinderHeight);
					r = radius * Mathf.Cos(i * Mathf.PI / (2 * (resolution -1)) );
					offset = i * circleVertices + k;
					for (int j = 0; j < circleVertices; ++j) {
						rad = 2 * Mathf.PI * j / (circleVertices-1);
						x = r * Mathf.Cos(rad);
						z = r * Mathf.Sin(rad);
						vertices[offset + j] = new Vector3(x, y, z);
					}
				}
			}
			vertices[hemiSphereVertexCount-1] = new Vector3(0f,0f,0f);
			vertices[2 * hemiSphereVertexCount -1] = new Vector3(0f, height, 0f);

			// 三角形の数は(リング数-1) * (リングの頂点数-1) * 6 + (リングの頂点数-1) * 3 * 2
			// = (リング数) * (リングの頂点数-1) * 6
			// リング数は90度の弧を作る頂点数をnとすると、(n-1)*2
			// リングの頂点数はn*4-3なので-1すると(n-1)*4
			int[] indices = new int[48 * (resolution-1) * (resolution-1)];

			int pointer = 0;
			for (int i = 0, offset = pointer; i < circleVertices-1; ++i, offset += 6) {
				indices[offset + 0] = i;
				indices[offset + 2] = i + 1;
				indices[offset + 1] = hemiSphereVertexCount + i;
				indices[offset + 3] = hemiSphereVertexCount + i;
				indices[offset + 4] = hemiSphereVertexCount + i + 1;
				indices[offset + 5] = i + 1;
			}
			pointer += 6 * (circleVertices-1);

			for (int k = 1, offset = pointer, n = 0; k <= 2; ++k) {
				for (int j = 0; j < resolution-2; ++j, n += circleVertices) {
					for(int i = 0; i < circleVertices-1; ++i, offset += 6) {
						indices[offset + 0] = n + i;
						indices[offset + k] = n + i + 1;
						indices[offset + 3-k] = n + circleVertices + i;
						indices[offset + 3] = n + circleVertices + i;
						indices[offset + 6-k] = n + circleVertices + i + 1;
						indices[offset + 3+k] = n + i + 1;
					}
				}


				for(int i = 0; i < circleVertices-1; ++i, offset += 3) {
					indices[offset + 0] = hemiSphereVertexCount * k - 1;
					indices[offset + k] = n + i;
					indices[offset + 3-k] = n + i + 1;
				}

				n = hemiSphereVertexCount;
			}

			Mesh mesh = new Mesh();
			mesh.SetVertices(vertices);
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);
			mesh.subMeshCount = 1;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		private interface IViewContents {
			GameObject GameObject { get; }
			void AddTo(PreviewScene scene);
			void OnDestroy();
		}

		private class CapsuleViewContents : IViewContents {
			GameObject IViewContents.GameObject => obj;
			private GameObject obj;
			private Mesh mesh;

			public CapsuleViewContents(
				Mesh mesh, 
				Material material, 
				Vector3 position, 
				Quaternion rotation,
				Vector3 textOffset,
				string name
			) {
				obj = new GameObject();
				obj.transform.SetPositionAndRotation(position, rotation);
				this.mesh = mesh;

				MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
				renderer.sharedMaterial = material;
				MeshFilter filter = obj.AddComponent<MeshFilter>();
				filter.mesh = mesh;

				GameObject textObj = new GameObject();
				textObj.transform.SetParent(obj.transform);
				textObj.transform.localPosition = textOffset;
				textObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				TextMesh textMesh = textObj.AddComponent<TextMesh>();
				textMesh.text = name;
				textMesh.color = new Color(material.color.r, material.color.g, material.color.b, 1.0f);
				textMesh.fontSize = 40;
				textMesh.alignment = TextAlignment.Center;
				textMesh.anchor = TextAnchor.MiddleCenter;
			}

			void IViewContents.AddTo(PreviewScene scene) {
				scene.Add(obj);
			}

			void IViewContents.OnDestroy() {
				DestroyImmediate(obj);
				DestroyImmediate(mesh);
			}
		}

		private class GameObjectContents : IViewContents {
			GameObject IViewContents.GameObject => obj;
			private GameObject obj;

			void IViewContents.AddTo(PreviewScene scene) {
				scene.Add(obj);
			}

			public GameObjectContents(GameObject obj) {
				this.obj = obj;
			}

			void IViewContents.OnDestroy() {
				DestroyImmediate(obj);
			}
		}

		private class FieldMeshContents : IViewContents {
			GameObject IViewContents.GameObject => obj;

			private GameObject obj;
			private GameObject[] meshObj;
			private Material material;
			private Mesh[] meshes;

			public Vector3 BoundsMin { get; private set; }
			public Vector3 BoundsMax { get; private set; }

			void IViewContents.AddTo(PreviewScene scene) {
				scene.Add(obj);
			}

			public FieldMeshContents(BaseFieldBlueprint bp) {
				obj = new GameObject();
				
				material = new Material(
					Shader.Find("Universal Render Pipeline/Lit")
				);

				BoundsMin = new Vector3(-10f, -10f, -10f);
				BoundsMax = new Vector3(10f, 10f, 10f);

				meshes = new Mesh[bp.MeshCount];
				meshObj = new GameObject[bp.MeshCount];
        
				for(int i = 0; i < meshes.Length; ++i) {
					if (bp.TryGetMesh(i, out Vector3[] vertices, out int[] indices)) {
						Vector3 boundsMin = BoundsMin;
						Vector3 boundsMax = BoundsMax;
						foreach(Vector3 vertex in vertices) {
							if (vertex.x < boundsMin.x) { boundsMin.x = vertex.x; }
							if (vertex.y < boundsMin.y) { boundsMin.y = vertex.y; }
							if (vertex.z < boundsMin.z) { boundsMin.z = vertex.z; }
							if (vertex.x > boundsMax.x) { boundsMax.x = vertex.x; }
							if (vertex.y > boundsMax.y) { boundsMax.y = vertex.y; }
							if (vertex.z > boundsMax.z) { boundsMax.z = vertex.z; }
						}
						BoundsMin = boundsMin;
						BoundsMax = boundsMax;

						meshes[i] = new Mesh();
						meshes[i].SetVertices(vertices);
						// PreviewViewで表示する場合subMeshCountの指定は必須
						meshes[i].subMeshCount = 1;
						meshes[i].SetIndices(indices, MeshTopology.Triangles, 0);
						meshes[i].RecalculateNormals();
						meshes[i].RecalculateBounds();
						
						meshObj[i] = new GameObject();
						meshObj[i].transform.SetParent(obj.transform);

						MeshRenderer renderer = meshObj[i].AddComponent<MeshRenderer>();
						renderer.sharedMaterial = material;

						MeshFilter filter = meshObj[i].AddComponent<MeshFilter>();
						filter.mesh = meshes[i];
					} else {
						meshes[i] = null;
						meshObj[i] = null;
					}
				}
			}

			void IViewContents.OnDestroy() {
				for(int i = 0; i < meshes.Length; ++i) {
					if (meshes[i] != null) {
						DestroyImmediate(meshes[i]);
						DestroyImmediate(meshObj[i]);
					}
				}
				DestroyImmediate(obj);
				DestroyImmediate(material);
			}
		}
	}
}