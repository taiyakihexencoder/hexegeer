using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.UIElements;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerContentKeySystemGroup))]
	public partial class ContentKeyLoadSystem : SystemBase {
		private const int GLOBAL_CONTENT_KEY = 0;

		private EntityQuery _query;
		private bool _initialized;

		private Entity _characterRootEntity;
		private ConcurrentDictionary<int, BlobAssetReference<Collider>> _characterColliders;
		private Dictionary<int, Entity> _characters;

		private Entity _damageObjectRootEntity;
		private ConcurrentDictionary<int, BlobAssetReference<Collider>> _damageObjectColliders;
		private Dictionary<int, Entity> _damageObjects;

		private EntityArchetype _characterEntryArchetype;
		private EntityArchetype _characterArchetype;
		private EntityArchetype _characterHitAreaArchetype;

		private EntityArchetype _damageObjectEntryArchetype;
		private EntityArchetype _damageObjectArchetype;

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
				ComponentType.ReadWrite<ColliderCollisionStayEvent>(),
				ComponentType.ReadWrite<LinkedEntityGroup>(),
				ComponentType.ReadOnly<PhysicsWorldIndex>()
			);

			_characterHitAreaArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<PhysicsCollider>(),
				ComponentType.ReadOnly<GeometryCleanup>(),
				ComponentType.ReadWrite<ColliderTriggerEvent>(),
				ComponentType.ReadWrite<ColliderTriggerEnterEvent>(),
				ComponentType.ReadOnly<PhysicsWorldIndex>()
			);

			_damageObjectEntryArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadWrite<DamageObjectPrefabEntry>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<LocalToWorld>()
			);

			_damageObjectArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadOnly<Prefab>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<PhysicsCollider>(),
				ComponentType.ReadWrite<PhysicsVelocity>(),
				ComponentType.ReadWrite<ColliderTriggerEvent>(),
				ComponentType.ReadWrite<ColliderTriggerEnterEvent>(),
				ComponentType.ReadOnly<PhysicsWorldIndex>(),
				ComponentType.ReadWrite<EntityOwner>(),
				ComponentType.ReadWrite<LimitedLifeTime>()
			);

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<ContentKeyLoadRequest>()
				.Build(EntityManager);
			RequireForUpdate(_query);

			RequireForUpdate<LayoutBlobTable>();
			RequireForUpdate<CharacterBlobTable>();
			RequireForUpdate<DamageObjectBlobTable>();
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

				_characterColliders = new ConcurrentDictionary<int, BlobAssetReference<Collider>>();
				_characters = new Dictionary<int, Entity>();

				_damageObjectRootEntity = EntityManager.Create(
					new LocalToWorld { Value = float4x4.identity, },
					LocalTransform.FromPosition(float3.zero),
					new Parent(),
					new AttachHexegeerTree()
				);
				ECS.SetEntityName(EntityManager, _damageObjectRootEntity, "DamageObjectPrefabs@Hexegeer");

				_damageObjectColliders = new ConcurrentDictionary<int, BlobAssetReference<Collider>>();
				_damageObjects = new Dictionary<int, Entity>();

				_initialized = true;
			}
		}

		protected override void OnDestroy() {
			foreach(KeyValuePair<int, BlobAssetReference<Collider>> collider in _characterColliders) {
				collider.Value.Dispose();
			}

			foreach(KeyValuePair<int, BlobAssetReference<Collider>> collider in _damageObjectColliders) {
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
			for (int i = 0; i < characterTable.loadTable.Value.rows.Length; ++i) {
				if (characterTable.loadTable.Value.rows[i].key == contentKey) {
					for (int j = 0; j < characterTable.loadTable.Value.rows[i].list.Length; ++j) {
						CharacterLoadElement element = characterTable.loadTable.Value.rows[i].list[j];
						CharacterInfo info = characterTable.character.Value.rows[element.index];
						ref BlobArray<CharacterHitArea> hitAreaArray = ref characterTable.character.Value.hitArea[element.index].list;
						if (!_characters.ContainsKey(info.id)) {
							LoadCharacterPrefab(
								entityManager, 
								info, 
								ref hitAreaArray,
								FindCollider(characterTable, info.collider),
								characterTable.physicsObjectLayer,
								characterTable.physicsObjectCollides
							);
						}
					}
					break;
				}
			}

			DamageObjectBlobTable damageObjectTable = SystemAPI.GetSingleton<DamageObjectBlobTable>();
			
			// Damage Object Table
			for (int i = 0; i < damageObjectTable.keyTable.Value.list.Length; ++i) {
				if (damageObjectTable.keyTable.Value.list[i].key == contentKey) {
					ref BlobArray<DamageObjectLoadElement> array = ref damageObjectTable.keyTable.Value.list[i].elements;
					for (int j = 0; j < array.Length; ++j) {
						DamageObjectInfo info = damageObjectTable.damageObject.Value.objectList[array[j].index];
						if (!_damageObjects.ContainsKey(info.id)) {
							LoadDamageObjectPrefab(entityManager, info, FindCollider(damageObjectTable, info.collider));
						}
					}
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
									ref BlobArray<CharacterHitArea> hitAreaArray = ref characterTable.character.Value.hitArea[index].list;
									LoadCharacterPrefab(
										entityManager,
										info,
										ref hitAreaArray,
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
			ref BlobArray<CharacterHitArea> hitAreaArray,
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
				entityManager.SetSharedComponentManaged(prefab, new PhysicsWorldIndex{ Value = 0, });
			}

			// HitArea
			List<Entity> hitAreaList = new List<Entity>();
			for (int i = 0; i < hitAreaArray.Length; ++i) {
				Entity hitAreaEntity = entityManager.CreateEntity(_characterHitAreaArchetype);
				ECS.SetEntityName(entityManager, hitAreaEntity, $"HitArea {i+1}");
				entityManager.SetComponentData(hitAreaEntity, new Parent{ Value = prefab, });
				entityManager.SetComponentData(hitAreaEntity, LocalTransform.Identity);
				BlobAssetReference<Collider> hitAreaCollider = CreateHitArea(info, hitAreaArray[i]);
				entityManager.SetComponentData(hitAreaEntity, new PhysicsCollider { Value = hitAreaCollider, });
				entityManager.SetComponentData(hitAreaEntity, new GeometryCleanup { geometry = hitAreaCollider, });
				entityManager.SetSharedComponentManaged(hitAreaEntity, new PhysicsWorldIndex{ Value = 0, });
				hitAreaList.Add(hitAreaEntity);
			}

			DynamicBuffer<LinkedEntityGroup> links = entityManager.GetBuffer<LinkedEntityGroup>(prefab);
			links.Add(new LinkedEntityGroup{ Value = prefab, });
			foreach(Entity hitAreaEntity in hitAreaList) {
				links.Add(hitAreaEntity);
			}

			_characters.Add(info.id, prefab);

		}

		private BlobAssetReference<Collider> LoadCollider(CharacterColliderInfo collider, int belongsTo, int collidesWith) {
			if (!_characterColliders.TryGetValue(collider.id, out BlobAssetReference<Collider> asset)) {
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

				_characterColliders.TryAdd(collider.id, capsule);
				asset = capsule;
			}
			return asset;
		}

		private BlobAssetReference<Collider> CreateHitArea(in CharacterInfo info, in CharacterHitArea area) {
			Material physicsMaterial = Material.Default;
			physicsMaterial.Friction = 0.0f;
			physicsMaterial.FrictionCombinePolicy = Material.CombinePolicy.Minimum;

			CollisionFilter filter = new CollisionFilter {
				BelongsTo = (uint)info.belongsTo,
				CollidesWith = (uint)info.collidesWith,
			};

			switch(area.shape) {
				case HitAreaShape.Sphere: {
					BlobAssetReference<Collider> sphere = SphereCollider.Create(
						geometry: new SphereGeometry {
							Radius = area.extent.x,
							Center = area.position,
						},
						filter: filter,
						material: physicsMaterial
					);
					sphere.Value.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
					return sphere;
				}

				case HitAreaShape.Box: {
					BlobAssetReference<Collider> box = BoxCollider.Create(
						geometry: new BoxGeometry {
							BevelRadius = 0f,
							Size = area.extent,
							Center = area.position,
							Orientation = area.rotation,
						},
						filter: filter,
						material: physicsMaterial
					);
					box.Value.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
					return box;
				}

				default: {
					return default;
				}
			}
		}

		private void SpawnCharacter(int characterId, float3 position, quaternion rotation) {
			if (_characters.TryGetValue(characterId, out Entity prefab)) {
				Entity instance = EntityManager.Instantiate(prefab);
				EntityManager.SetComponentData(instance, LocalTransform.FromPositionRotation(position, rotation));
				EntityManager.RemoveComponent<Parent>(instance);
			}
		}

		private DamageObjectColliderInfo? FindCollider(in DamageObjectBlobTable table, int id) {
			for (int i = 0; i < table.damageObject.Value.colliderList.Length; ++i) {
				if (table.damageObject.Value.colliderList[i].id == id) {
					return table.damageObject.Value.colliderList[i];
				}
			}
			return null;
		}

		private void LoadDamageObjectPrefab(
			EntityManager entityManager,
			in DamageObjectInfo info,
			in DamageObjectColliderInfo? colliderInfo
		) {
			Entity entry = entityManager.CreateEntity(_damageObjectEntryArchetype);
			ECS.SetEntityName(entityManager, entry, $"Prefab Entry - {info.name}");

			Entity prefab = entityManager.CreateEntity(_damageObjectArchetype);
			ECS.SetEntityName(entityManager, prefab, info.name);
	
			ECS.SetComponents(
				entityManager,
				entry,
				LocalTransform.FromPositionRotation(float3.zero, quaternion.identity),
				new LocalToWorld { Value = float4x4.identity, },
				new Parent { Value = _damageObjectRootEntity, },
				new DamageObjectPrefabEntry { id = info.id, prefab = prefab, }
			);

			ECS.SetComponents(
				entityManager,
				prefab,
				LocalTransform.FromPositionRotation(float3.zero, quaternion.identity),
				new LocalToWorld { Value = float4x4.identity, },
				new Parent { Value = entry, }
			);

			if (colliderInfo == null) {
				entityManager.RemoveComponent<PhysicsCollider>(prefab);
			} else {
				ECS.SetComponents(
					entityManager,
					prefab,
					new PhysicsCollider { Value = LoadDamageObjectCollider(info, colliderInfo.Value), }
				);
			}

			entityManager.SetSharedComponentManaged(prefab, new PhysicsWorldIndex{ Value = 0, });
			_damageObjects.Add(info.id, prefab);
		}

		private BlobAssetReference<Collider> LoadDamageObjectCollider(in DamageObjectInfo info, in DamageObjectColliderInfo collider) {
			if (!_damageObjectColliders.TryGetValue(info.collider, out BlobAssetReference<Collider> asset)) {

				// なければ作成
				Material physicsMaterial = Material.Default;
				physicsMaterial.Friction = 0.0f;
				physicsMaterial.FrictionCombinePolicy = Material.CombinePolicy.Minimum;

				CollisionFilter filter = new CollisionFilter {
					BelongsTo = (uint)collider.belongsTo,
					CollidesWith = (uint)collider.collidesWith,
				};

				switch(collider.shape) {
					case HitAreaShape.Sphere: {
						asset = SphereCollider.Create(
							geometry: new SphereGeometry {
								Radius = collider.extent.x,
								Center = float3.zero,
							},
							filter: filter,
							material: physicsMaterial
						);
						break;
					}

					case HitAreaShape.Box: {
						asset = BoxCollider.Create(
							geometry: new BoxGeometry {
								BevelRadius = 0f,
								Size = collider.extent,
								Center = float3.zero,
								Orientation = collider.rotation,
							},
							filter: filter,
							material: physicsMaterial
						);
						break;
					}

					case HitAreaShape.Cylinder: {
						asset = CylinderCollider.Create(
							geometry: new CylinderGeometry {
								BevelRadius = 0f,
								Radius = collider.extent.x,
								Height = collider.extent.y,
								Center = float3.zero,
								Orientation = collider.rotation,
								SideCount = 12,
							},
							filter: filter,
							material: physicsMaterial
						);
						break;
					}

					default: {
						return default;
					}

				}

				_damageObjectColliders.TryAdd(info.collider, asset);
			}
			asset.Value.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
			return asset;
		}
	}
}
