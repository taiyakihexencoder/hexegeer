using System.IO;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	internal sealed class FieldResourceGenerator : ResourceGenerator<FieldMeshResource> {
		private BaseFieldBlueprint blueprint;

		internal override string AutoGeneratePath => base.AutoGeneratePath + blueprint.GetType().Name + Path.DirectorySeparatorChar;

		public FieldResourceGenerator(BaseFieldBlueprint blueprint) {
			this.blueprint = blueprint;
		}

		protected override void Edit(SerializedObject serializedObject) {
			SerializedProperty guidProperty = serializedObject.FindProperty("_guid");
			SerializedProperty subassetsProperty = serializedObject.FindProperty("_subassets");

			string assetPath = AssetDatabase.GetAssetPath(blueprint);
			string guid = AssetDatabase.AssetPathToGUID(assetPath);

			guidProperty.stringValue = guid;
			subassetsProperty.arraySize = blueprint.MeshCount;

			// 各メッシュを作成してサブアセットへ
			for(int i = 0, iMax = blueprint.MeshCount; i < iMax; ++i) {
				if (blueprint.TryGetMesh(i, out Vector3[] vertices, out int[] indices)) {
					if (vertices.Length >= 2) {
						string name = blueprint.GetName(i);
						if (string.IsNullOrEmpty(name)) {
							name = $"NoName{i.ToString("00")}";
						}
						Mesh mesh = new Mesh();
						mesh.name = name;
						mesh.SetVertices(vertices);
						mesh.SetIndices(indices, MeshTopology.Triangles, 0);
						mesh.RecalculateNormals();
						AssetDatabase.AddObjectToAsset(mesh, serializedObject.targetObject);
						subassetsProperty.GetArrayElementAtIndex(i).stringValue = name;
					}
				}
			}

			// set address
			SetAddress(serializedObject, blueprint.RuntimeAssetAddress);
			AssetDatabase.SaveAssets();
		}
	}
}