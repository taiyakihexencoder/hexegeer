using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerContentKeySystemGroup))]
	public partial class ContentKeyLoadSystem : SystemBase {
		private const int GLOBAL_CONTENT_KEY = 0;

		private EntityQuery _query;
		private bool _initialized;

		private Entity _characterRootEntity;
		private ConcurrentDictionary<int, BlobAssetReference<Collider>> _colliders;
		private Dictionary<int, Entity> _characters;

		private EntityArchetype _characterEntryArchetype;
		private EntityArchetype _characterArchetype;

		override protected void OnCreate() {
			_initialized = false;

			_characterEntryArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadWrite<CharacterPrefabEntry>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<LocalToWorld>()
			);

			_characterArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadOnly<Prefab>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<CharacterHeader>(),
				ComponentType.ReadWrite<CharacterMoveStatus>(),
				ComponentType.ReadWrite<CharacterGroundedStatus>(),
				ComponentType.ReadWrite<PhysicsCollider>(),
				ComponentType.ReadWrite<PhysicsGravityFactor>(),
				ComponentType.ReadWrite<PhysicsMass>(),
				ComponentType.ReadWrite<PhysicsVelocity>(),
				ComponentType.ReadWrite<ColliderCollisionEvent>(),
				ComponentType.ReadWrite<ColliderCollisionStayEvent>()
			);

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<ContentKeyLoadRequest>()
				.Build(EntityManager);
			RequireForUpdate(_query);

			RequireForUpdate<LayoutBlobTable>();
			RequireForUpdate<CharacterBlobTable>();
		}

		protected override void OnStartRunning() {
			if (!_initialized) {
				_characterRootEntity = EntityManager.Create(
					new LocalToWorld { Value = float4x4.identity, },
					LocalTransform.FromPosition(float3.zero),
					new Parent(),
					new AttachHexegeerTree()
				);
				ECS.SetEntityName(EntityManager, _characterRootEntity, "CharacterPrefabs@Hexegeer");

				_colliders = new ConcurrentDictionary<int, BlobAssetReference<Collider>>();
				_characters = new Dictionary<int, Entity>();

				_initialized = true;
			}
		}

		protected override void OnDestroy() {
			foreach(KeyValuePair<int, BlobAssetReference<Collider>> collider in _colliders) {
				collider.Value.Dispose();
			}
		}

		override protected void OnUpdate() {
			NativeArray<ContentKeyLoadRequest> requests = _query.ToComponentDataArray<ContentKeyLoadRequest>(Allocator.Temp);
			int[] keys = new int[requests.Length];
			for (int i = 0; i < requests.Length; ++i) {
				keys[i] = requests[i].contentKey;
			}
			requests.Dispose();

			for (int i = 0; i < keys.Length; ++i) {
				LoadContentKeyResource(EntityManager, keys[i]);
			}

			EntityManager.DestroyEntity(_query);
		}

		private void LoadContentKeyResource(EntityManager entityManager, int contentKey) {
			bool global = (contentKey == GLOBAL_CONTENT_KEY);

			CharacterBlobTable characterTable = SystemAPI.GetSingleton<CharacterBlobTable>();

			// Character Table
			for(int i = 0; i < characterTable.loadTable.Value.rows.Length; ++i) {
				if (characterTable.loadTable.Value.rows[i].key == contentKey) {
					for (int j = 0; j < characterTable.loadTable.Value.rows[i].list.Length; ++j) {
						CharacterLoadElement element = characterTable.loadTable.Value.rows[i].list[j];
						CharacterInfo info = characterTable.character.Value.rows[element.index];
						if (!_characters.ContainsKey(info.id)) {
							LoadCharacterPrefab(
								entityManager, 
								info, 
								FindCollider(characterTable, info.collider),
								characterTable.physicsObjectLayer,
								characterTable.physicsObjectCollides
							);
						}
					}
					break;
				}
			}

			// レイアウトテーブル
			// Globalに対応するレイアウトデータは設定されていない
			if (!global) {
				LayoutBlobTable layoutTable = SystemAPI.GetSingleton<LayoutBlobTable>();
				for (int i = 0; i < layoutTable.asset.Value.rows.Length; ++i) {
					if (layoutTable.asset.Value.rows[i].contentKey == contentKey) {
						// Load CharacterPrefabs
						for (int j = 0; j < layoutTable.asset.Value.rows[i].loadCharacters.Length; ++j) {
							LayoutLoadCharacterInfo loadInfo = layoutTable.asset.Value.rows[i].loadCharacters[j];
							if (!_characters.ContainsKey(loadInfo.id)) {
								if (TryGetCharacter(characterTable, loadInfo.id, out int index)) {
									CharacterInfo info = characterTable.character.Value.rows[index];
									LoadCharacterPrefab(
										entityManager,
										info,
										FindCollider(characterTable, info.collider),
										characterTable.physicsObjectLayer,
										characterTable.physicsObjectCollides
									);
								}
							}
						}

						// Spawn Characters
						for (int j = 0; j < layoutTable.asset.Value.rows[i].characterLayout.Length; ++j) {
							LayoutCharacterInfo characterInfo = layoutTable.asset.Value.rows[i].characterLayout[j];
							SpawnCharacter(characterInfo.id, characterInfo.position, characterInfo.rotation);
						}
					}
				}
			}
		}

		private bool TryGetCharacter(in CharacterBlobTable table, int id, out int index) {
			for (int i = 0; i < table.character.Value.rows.Length; ++i) {
				if (table.character.Value.rows[i].id == id) {
					index = i;
					return true;
				}
			}
			index = -1;
			return false;
		}

		private CharacterColliderInfo? FindCollider(in CharacterBlobTable table, int id) {
			for (int i = 0; i < table.collider.Value.rows.Length; ++i) {
				if (table.collider.Value.rows[i].id == id) {
					return table.collider.Value.rows[i];
				}
			}
			return null;
		}

		private void LoadCharacterPrefab(
			EntityManager entityManager, 
			in CharacterInfo info, 
			in CharacterColliderInfo? collider,
			int belongsTo,
			int collidesWith
		) {
			Entity entry = entityManager.CreateEntity(_characterEntryArchetype);
			ECS.SetEntityName(entityManager, entry, $"Prefab Entry - {info.name}");

			Entity prefab = entityManager.CreateEntity(_characterArchetype);
			ECS.SetEntityName(entityManager, prefab, info.name);

			ECS.SetComponents(
				entityManager,
				entry,
				LocalTransform.FromPositionRotation(float3.zero, quaternion.identity),
				new LocalToWorld { Value = float4x4.identity, },
				new Parent { Value = _characterRootEntity, },
				new CharacterPrefabEntry { id = info.id, prefab = prefab, }
			);

			ECS.SetComponents(
				entityManager,
				prefab,
				LocalTransform.FromPositionRotation(float3.zero, quaternion.identity),
				new LocalToWorld { Value = float4x4.identity, },
				new CharacterHeader { 
					id = info.id, 
				},
				new Parent { Value = entry, }
			);

			if (info.hasObservationPoint) {
				entityManager.AddComponent<FieldObservationPoint>(prefab);
			}

			// Motion Settings
			ECS.SetComponents(
				entityManager,
				prefab,
				new CharacterMoveStatus { 
					lookDirectionThreshold = 0.2f, 
					correctionSeconds = 0.3f,
					lookDirection = quaternion.identity,
				}
			);

			// Collider
			if (collider == null) {
				entityManager.RemoveComponents<PhysicsCollider, PhysicsGravityFactor, CharacterGroundedStatus>(prefab);
			} else {
				PhysicsMass mass = PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 50.0f);
				mass.InverseInertia = new float3(0f, 1f, 0f);
				ECS.SetComponents(
					entityManager,
					prefab,
					new PhysicsCollider { Value = LoadCollider(collider.Value, belongsTo, collidesWith), },
					new PhysicsGravityFactor { Value = 3.0f, },
					mass
				);
				ECS.SetComponents(
					entityManager,
					prefab,
					new CharacterGroundedStatus {
						groundThreshold = math.cos(math.radians(35f)),
						normal = new float3(0f, 1f, 0f),
					}
				);
				entityManager.AddSharedComponent(prefab, new PhysicsWorldIndex{ Value = 0, });
			}

			_characters.Add(info.id, prefab);

		}

		private BlobAssetReference<Collider> LoadCollider(CharacterColliderInfo collider, int belongsTo, int collidesWith) {
			if (!_colliders.TryGetValue(collider.id, out BlobAssetReference<Collider> asset)) {
				Material physicsMaterial = Material.Default;
				physicsMaterial.Friction = 0.0f;
				physicsMaterial.FrictionCombinePolicy = Material.CombinePolicy.Minimum;

				BlobAssetReference<Collider> capsule = CapsuleCollider.Create(
					geometry: new CapsuleGeometry {
						Radius = collider.radius,
						Vertex0 = new float3(0.0f, collider.radius, 0.0f),
						Vertex1 = new float3(0.0f, collider.height - collider.radius, 0.0f),
					},
					filter: new CollisionFilter {
						BelongsTo = (uint) belongsTo,
						CollidesWith = (uint) collidesWith,
					},
					material: physicsMaterial
				);
				capsule.Value.SetCollisionResponse(CollisionResponsePolicy.CollideRaiseCollisionEvents);

				_colliders.TryAdd(collider.id, capsule);
				asset = capsule;
			}
			return asset;
		}

		private void SpawnCharacter(int characterId, float3 position, quaternion rotation) {
			if (_characters.TryGetValue(characterId, out Entity prefab)) {
				Entity instance = EntityManager.Instantiate(prefab);
				EntityManager.SetComponentData(instance, LocalTransform.FromPositionRotation(position, rotation));
				EntityManager.RemoveComponent<Parent>(instance);
			}
		}
	}
}
