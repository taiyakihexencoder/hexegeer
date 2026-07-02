using UnityEngine;

namespace hexegeer.editor {
	[CreateAssetMenu(fileName = "SideViewFieldBlueprint", menuName = "Hexegeer/Field Blueprint/Side view")]
	internal sealed class SideViewFieldBlueprint : BaseFieldBlueprint {
		[SerializeField]
		private Vector2 _rootPosition = Vector2.zero;
		public Vector2 RootPosition => _rootPosition;

		[SerializeField]
		private FieldElement[] _fieldElements = new FieldElement[0];
		public FieldElement[] FieldElements => _fieldElements;

		[System.Serializable]
		public sealed class FieldElement {
			[SerializeField]
			private string _name = "";
			public string Name => _name;

			[SerializeField]
			private bool _visible = true;
			public bool Visible => _visible;

			[SerializeField]
			private Vector2[] _points;
			public Vector2[] Points => _points;
		}

		public override Vector3 Position => new Vector3(_rootPosition.x, _rootPosition.y, FieldSideViewSettings.instance.ZOffset);
		public override Quaternion Rotation => Quaternion.identity;
		public override int MeshCount => _fieldElements.Length;
		public override string GetName(int index) {
			if (index < 0 || _fieldElements.Length <= index) {
				return "";
			} else {
				return _fieldElements[index].Name;
			}
		}

		public override bool TryGetMesh(int index, out Vector3[] vertices, out int[] indices){
			if (index < 0 || _fieldElements.Length <= index) {
				vertices = new Vector3[0];
				indices = new int[0];
				return false;
			} else {
				Vector2[] points = _fieldElements[index].Points;
				if (points.Length < 2) {
					vertices = new Vector3[0];
					indices = new int[0];
					return false;
				} else {
					int vertexCount = (points.Length-1) * 6;
					vertices = new Vector3[vertexCount];
					indices = new int[vertexCount];

					FieldSideViewSettings sideViewSettings = FieldSideViewSettings.instance;

					float zOffset = sideViewSettings.ZOffset;
					float halfWidth = sideViewSettings.Width * 0.5f;

					for(int i = 1, n = 0; i < points.Length; ++i, n += 6) {
						vertices[n] = new Vector3(points[i-1].x, points[i-1].y, -halfWidth + zOffset);
						vertices[n+1] = new Vector3(points[i-1].x, points[i-1].y, halfWidth + zOffset);
						vertices[n+2] = new Vector3(points[i].x, points[i].y, halfWidth + zOffset);
						vertices[n+3] = new Vector3(points[i].x, points[i].y, halfWidth + zOffset);
						vertices[n+4] = new Vector3(points[i].x, points[i].y, -halfWidth + zOffset);
						vertices[n+5] = new Vector3(points[i-1].x, points[i-1].y, -halfWidth + zOffset);

						indices[n] = n;
						indices[n+1] = n+1;
						indices[n+2] = n+2;
						indices[n+3] = n+3;
						indices[n+4] = n+4;
						indices[n+5] = n+5;
					}
					return true;
				}
			}
		}

		public override bool IsVisible(int index) {
			return 0 <= index && index < _fieldElements.Length && _fieldElements[index].Visible;
		}
	}
}
