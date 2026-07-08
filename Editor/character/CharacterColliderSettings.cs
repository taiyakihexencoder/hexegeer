using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;

namespace hexegeer.editor {
	[FilePath("Hexegeer/Character/CharacterColliderSettings.geer", FilePathAttribute.Location.ProjectFolder)]
	internal sealed class CharacterColliderSettings : ScriptableSingleton<CharacterColliderSettings> {
		[System.Serializable]
		internal class PhysicsCollider {
			internal int id;
			internal string name;
			internal float radius;
			internal float height;
		}

		[SerializeField]
		private PhysicsCollider[] _physicsColliders;
		internal PhysicsCollider[] PhysicsColliders => _physicsColliders;

		internal int Add() {
			PhysicsCollider[] newList = new PhysicsCollider[_physicsColliders.Length+1];
			System.Array.Copy(_physicsColliders, newList, _physicsColliders.Length);

			List<int> ids = new List<int>();
			foreach(PhysicsCollider collider in _physicsColliders) {
				ids.Add(collider.id);
			}
			ids.Sort();

			int id = 1;
			foreach(int current in ids) {
				if (current - id > 0) {
					break;
				}
				id = current;
			}

			newList[_physicsColliders.Length] = new PhysicsCollider {
				id = id,
				name = "new collider",
				radius = 0.5f,
				height = 2f,
			};
			_physicsColliders = newList;
			Save(true);
			return id;
		}

		internal void Remove(int id) {
			for(int i = 0; i < _physicsColliders.Length; ++i) {
				if (_physicsColliders[i].id == id) {
					PhysicsCollider[] newList = new PhysicsCollider[_physicsColliders.Length-1];
					System.Array.Copy(_physicsColliders, newList, i);
					System.Array.Copy(_physicsColliders, i+1, newList, i, newList.Length-i);
					_physicsColliders = newList;
					Save(true);
					return;
				}
			}
		}

		internal void UpdateCollider(PhysicsCollider collider) {
			for (int i = 0; i < _physicsColliders.Length; ++i) {
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

				PhysicsCollider[] colliders = instance.PhysicsColliders;
				foreach(PhysicsCollider collider in colliders) {
					if (collider.id == key) { return collider.name; }
				}
				return " - ";
			});

			return UpdateKeys(builder);
		}

		internal ListPopupBuilder<int> UpdateKeys(ListPopupBuilder<int> builder) {
			List<int> list = new List<int>();
			list.Add(0);
			foreach(PhysicsCollider collider in instance.PhysicsColliders) {
				list.Add(collider.id);
			}

			return builder.SetKeys(list);
		}
	}
}