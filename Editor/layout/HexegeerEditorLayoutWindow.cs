using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorLayoutWindow : PreviewWindow {		
		protected override Rect PreviewRect => new Rect(
			new Vector2(5f, 5f),
			position.size - new Vector2(10f, 10f)
		);

		private ListPopupBuilder<int> _contentKeyPopupBuilder;

		private bool _openDetail;
		private bool _initialized;

		private List<BaseFieldBlueprint> _blueprints = new List<BaseFieldBlueprint>();
		private List<IViewContents> _viewContents = new List<IViewContents>();

		protected override void OnEnablePreview() {
			titleContent = new GUIContent("Layout");

			_initialized = false;

			_contentKeyPopupBuilder = ContentKeySetting.instance.CreateListPopupBuilder();

			VisualElement rootView = CreateView();
			rootView.pickingMode = PickingMode.Ignore;

			rootVisualElement.Add(rootView);
			rootView.StretchToParentSize();
		}

		private void OnFocus() {
			_openDetail = false;
			
			if (!_initialized) {
				_initialized = true;

				FieldMainSettings fieldSettings = FieldMainSettings.instance;
				System.Type type = fieldSettings.ViewType.GetResourceType();
				foreach(string guid in AssetDatabase.FindAssets($"t:{type.Name}")) {
					string assetPath = AssetDatabase.GUIDToAssetPath(guid);
					_blueprints.Add(AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath));
				}
			}

			_contentKeyPopupBuilder = ContentKeySetting.instance.UpdateKeys(_contentKeyPopupBuilder);

			previewPaused = false;

			camera.fov = 60f;
		}

		private void OnLostFocus() {
			previewPaused = true;
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

				PopupField<int> contentKeyPopup = _contentKeyPopupBuilder.Generate(ContentKey.Global.value);
				popupElement.Add(contentKeyPopup);
				contentKeyPopup.RegisterValueChangedCallback(v => {
					OnSelectContentKey(v.newValue);
				});

				overlayView.Add(popupElement);
			}

			// detail view
			if (_openDetail) {
				// item list
				{
					ScrollPane listPane = new ScrollPane()
						.Background(new Color(0.0f, 0.0f, 0.0f, 0.5f))
						.Border(Color.white, 1f, 12f);
					listPane.style.height = new Length(98f, LengthUnit.Percent);
					listPane.style.position = Position.Absolute;
					listPane.style.left = 10f;
					listPane.style.width = new Length(30f, LengthUnit.Percent);
					listPane.style.top = new Length(1f, LengthUnit.Percent);
					listPane.style.bottom = new Length(1f, LengthUnit.Percent);
					listPane.style.opacity = 0.75f;

					listPane.Add(new Spacer(height: 50f));

					overlayView.Add(listPane);
				}

				// close button
				{
					ClickButton button = ClickButton.Create()
						.Label("-");
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
					.Label("+");
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
			foreach(IViewContents content in _viewContents) {
				content.OnDestroy();
			}
			_viewContents.Clear();

			if (contentKey != ContentKey.Global.value) {
				FieldMainSettings mainSettings = FieldMainSettings.instance;
				FieldViewType viewType = mainSettings.ViewType;

				BaseFieldBlueprint bp = _blueprints.Find(_ => _.ContentKey == contentKey);
				if (bp != null) {
					FieldMeshContents field = new FieldMeshContents(bp);

					// ビューの調整

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

					_viewContents.Add(field);
				}
			}

			foreach(IViewContents content in _viewContents) {
				content.AddTo(scene);
			}
		}

		private interface IViewContents {
			void AddTo(PreviewScene scene);
			void OnDestroy();
		}

		private class GameObjectContents : IViewContents {
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