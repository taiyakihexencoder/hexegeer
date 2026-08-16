using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/DamageObject/DamageObjectColliderSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class DamageObjectColliderSettings : ScriptableSingleton<DamageObjectColliderSettings> {
		[System.Serializable]
		public struct Geometry {
			public int id;
			public string name;
			public int layer;
			public GeometryShape shape;
			public Vector3 extent;
			public Quaternion rotation;
		}

		public enum GeometryShape {
			Sphere,
			Box,
			Cylinder,
		}

		[SerializeField]
		private List<Geometry> _colliders;
		public List<Geometry> Colliders {
			get {
				if (_colliders == null) { _colliders = new List<Geometry>(); }
				return _colliders;
			}
		}

		public void RemoveCollider(int id) {
			_colliders.RemoveAll(_ => _.id == id);
			Save(true);
		}

		public int AddCollider() {
			List<int> ids = _colliders.ConvertAll(_ => _.id);
			ids.Sort();
			int newId = 1;
			foreach (int id in ids) {
				if (newId < id) {
					break;
				}
				newId = id + 1;
			}

			_colliders.Add(
				new Geometry {
					id = newId,
					name = "newCollider",
					layer = LayerSettings.GetSelectableFirstLayer(),
					shape = GeometryShape.Sphere,
					extent = Vector3.one,
					rotation = Quaternion.identity,
				}
			);
			Save(true);
			return newId;
		}

		public void UpdateCollider(Geometry geometry) {
			for (int i = 0; i < _colliders.Count; ++i) {
				if (_colliders[i].id == geometry.id) {
					_colliders[i] = geometry;
					break;
				}
			}
			Save(true);
		}

		internal ListPopupBuilder<int> CreateListPopupBuilder() {
			ListPopupBuilder<int> builder = new ListPopupBuilder<int>();
			builder.SetConverter(key => {
				if (key == 0) { return "None"; }
				for (int i = 0; i < Colliders.Count; ++i) {
					if (Colliders[i].id == key) {
						return string.IsNullOrEmpty(Colliders[i].name) ? " - " : Colliders[i].name;
					}
				}
				return " - ";
			});
			return UpdateKeys(builder);
		}

		internal ListPopupBuilder<int> UpdateKeys(ListPopupBuilder<int> builder) {
			List<int> list = new List<int>{ 0 };
			list.AddRange(Colliders.ConvertAll(_ => _.id));
			return builder.SetKeys(list);
		}

		internal Geometry? Get(int id) {
			int index = _colliders.FindIndex(_ => _.id == id);
			return index >= 0 ? _colliders[index] : null;
		}
	}
}