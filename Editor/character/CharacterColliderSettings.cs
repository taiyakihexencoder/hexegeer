using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Character/CharacterColliderSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	public sealed class CharacterColliderSettings : ScriptableSingleton<CharacterColliderSettings> {
		[System.Serializable]
		public class PhysicsCollider {
			public int id;
			public string name;
			public float radius;
			public float height;
		}

		[SerializeField]
		private List<PhysicsCollider> _physicsColliders;
		public List<PhysicsCollider> PhysicsColliders => _physicsColliders;

		internal int Add() {
			List<int> ids = _physicsColliders.Map(_ => _.id);
			ids.Sort();

			int id = 1;
			foreach(int current in ids) {
				if (current - id > 0) {
					break;
				}
				id = current+1;
			}

			_physicsColliders.Add(
				new PhysicsCollider {
					id = id,
					name = "new collider",
					radius = 0.5f,
					height = 2f,
				}
			);

			Save(true);
			return id;
		}

		internal void Remove(int id) {
			_physicsColliders.RemoveAll(_ => _.id == id);
			Save(true);
		}

		internal void UpdateCollider(PhysicsCollider collider) {
			for (int i = 0; i < _physicsColliders.Count; ++i) {
				if (collider.id == _physicsColliders[i].id) {
					_physicsColliders[i] = collider;
					Save(true);
					return;
				}
			}
		}

		internal ListPopupBuilder<int> CreateListPopupBuilder() {
			ListPopupBuilder<int> builder = new ListPopupBuilder<int>();

			builder.SetConverter(key => {
				if (key == 0) { return "None"; }
				return PhysicsColliders.Find(_ => _.id == key)?.name ?? " - ";
			});

			return UpdateKeys(builder);
		}

		internal ListPopupBuilder<int> UpdateKeys(ListPopupBuilder<int> builder) {
			List<int> list = new List<int>();
			list.Add(0);
			foreach(PhysicsCollider collider in PhysicsColliders) {
				list.Add(collider.id);
			}

			return builder.SetKeys(list);
		}
	}
}