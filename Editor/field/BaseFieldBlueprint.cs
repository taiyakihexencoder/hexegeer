using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	internal abstract class BaseFieldBlueprint : ScriptableObject {
		[SerializeField]
		private string _runtimeAssetAddress = "";
		public string RuntimeAssetAddress => _runtimeAssetAddress;

		[SerializeField]
		private int _id = 0;
		public int Id => _id;

		public abstract Vector3 Position { get; }
		public abstract Quaternion Rotation { get; }
		public abstract int MeshCount { get; }
		public abstract string GetName(int index);
		public abstract bool TryGetMesh(int index, out Vector3[] vertices, out int[] indices);
		public abstract bool IsVisible(int index);
	}

	[CustomEditor(typeof(BaseFieldBlueprint), editorForChildClasses: true)]
	internal class FieldBlueprintEditor : Editor {
		private Mesh[] meshes = new Mesh[0];

		private void OnEnable() {
			SceneView.duringSceneGui += SceneGUI;
			UpdateMesh();
		}

		private void OnDisable() {
			SceneView.duringSceneGui -= SceneGUI;
			for(int i = 0; i < meshes.Length; ++i) {
				DestroyImmediate(meshes[i]);
			}
			meshes = new Mesh[0];
		}

		public override void OnInspectorGUI() {
			using (var scope = new EditorGUI.ChangeCheckScope()) {
				base.OnInspectorGUI();
				if (scope.changed) {
					UpdateMesh();
				}
			}
		}

		private void SceneGUI(SceneView sceneView) {
			BaseFieldBlueprint blueprint = target as BaseFieldBlueprint;

			Vector3 position = blueprint.Position;
			Handles.SphereHandleCap(0, position, Quaternion.identity, 1.0f, EventType.Repaint);
			int meshCount = blueprint.MeshCount;
			for(int i = 0; i < meshCount; ++i) {
				if (blueprint.IsVisible(i) && i < meshes.Length) {
					Graphics.DrawMeshNow(meshes[i], Matrix4x4.Translate(position), -1);
				}
			}
		}

		private void UpdateMesh() {
			BaseFieldBlueprint blueprint = target as BaseFieldBlueprint;
			List<Mesh> meshList = new List<Mesh>();
			int meshCount = blueprint.MeshCount;
			for(int i = 0; i < meshCount; ++i) {
				if (blueprint.TryGetMesh(i, out Vector3[] vertices, out int[] indices)) {
					Mesh mesh = new Mesh();
					mesh.SetVertices(vertices);
					mesh.SetIndices(indices, MeshTopology.Triangles, 0);
					mesh.RecalculateNormals();
					meshList.Add(mesh);
				}
			}

			for (int i = 0; i < meshes.Length; ++i) {
				DestroyImmediate(meshes[i]);
			}
			meshes = meshList.ToArray();

		}
	}

}