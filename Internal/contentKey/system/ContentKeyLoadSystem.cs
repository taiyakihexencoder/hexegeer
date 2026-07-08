using System.Collections.Concurrent;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace hexegeer.internallib {
	[UpdateInGroup(typeof(HexegeerContentKeySystemGroup))]
	public partial class ContentKeyLoadSystem : SystemBase {
		private EntityQuery _query;
		private bool _initialized;

		private Entity _characterRootEntity;
		private CharacterTable _characterTable;
		private ConcurrentDictionary<int, BlobAssetReference<Collider>> _colliders;
		private ConcurrentDictionary<int, Entity> _characters;

		private EntityArchetype _characterArchetype;

		override protected void OnCreate() {
			_initialized = false;

			_characterArchetype = EntityManager.CreateArchetype(
				ComponentType.ReadOnly<Prefab>(),
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<Parent>(),
				ComponentType.ReadWrite<CharacterHeader>(),
				ComponentType.ReadWrite<PhysicsCollider>(),
				ComponentType.ReadWrite<PhysicsGravityFactor>(),
				ComponentType.ReadWrite<PhysicsMass>(),
				ComponentType.ReadWrite<PhysicsVelocity>()
			);

			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<ContentKeyLoadRequest>()
				.Build(EntityManager);
			RequireForUpdate(_query);
		}

		protected override void OnStartRunning() {
			if (!_initialized) {
				_characterRootEntity = EntityManager.Create(
					new LocalToWorld { Value = float4x4.identity, },
					LocalTransform.FromPosition(float3.zero)
				);
				ECS.SetEntityName(EntityManager, _characterRootEntity, "CharacterPrefabs@Hexegeer");

				_colliders = new ConcurrentDictionary<int, BlobAssetReference<Collider>>();
				_characters = new ConcurrentDictionary<int, Entity>();

				_initialized = true;

				Task.Run(LoadTables);
			}
		}

		override protected void OnUpdate() {
			if (_characterTable == null) { return; }

			NativeArray<ContentKeyLoadRequest> requests = _query.ToComponentDataArray<ContentKeyLoadRequest>(Allocator.Temp);
			int[] keys = new int[requests.Length];
			for (int i = 0; i < requests.Length; ++i) {
				keys[i] = requests[i].contentKey;
			}
			requests.Dispose();

			for (int i = 0; i < keys.Length; ++i) {
				Task.Run(async () => await LoadKeyContents(keys[i]));
			}

			EntityManager.DestroyEntity(_query);
		}

		private async Task LoadTables() {
			_characterTable = await AssetUtil.RequestLoad<CharacterTable>(CharacterTable.RESOURCE_ADDRESS);
		}

		private async Task LoadKeyContents(int key) {
			// Character
			foreach(CharacterTable.KeyTable table in _characterTable.KeyTables) {
				if (table.key == key) {
					for (int i = 0; i < table.characterIndices.Length; ++i) {
						CharacterTable.Character character = _characterTable.Characters[i];
						Entity prefab = await LoadCharacter(character);
					}
					break;
				}
			}
		}

		private async Task<Entity> LoadCharacter(CharacterTable.Character character) {
			if (!_characters.TryGetValue(character.id, out Entity entity)) {
				await Task.Yield();

				// Entity生成
				SyncContext.Send(() => {
					Entity prefab = EntityManager.CreateEntity(_characterArchetype);
					ECS.SetEntityName(EntityManager, prefab, character.name);

					ECS.SetComponents(
						EntityManager,
						prefab,
						LocalTransform.FromPositionRotation(float3.zero, quaternion.identity),
						new LocalToWorld { Value = float4x4.identity, },
						new CharacterHeader { id = character.id, },
						new Parent { Value = _characterRootEntity, }
					);

					if (character.hasObservationPoint) {
						EntityManager.AddComponent<FieldObservationPoint>(prefab);
					}

					// Collider
					CharacterTable.CharacterCollider collider = _characterTable.GetCollider(character.id);
					if (collider == null) {
						EntityManager.RemoveComponents<PhysicsCollider, PhysicsGravityFactor, PhysicsMass, PhysicsVelocity>(prefab);
					} else {
						ECS.SetComponents(
							EntityManager,
							prefab,
							new PhysicsCollider { Value = LoadCollider(collider), },
							new PhysicsGravityFactor { Value = 3.0f, },
							PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 50.0f)
						);
						EntityManager.AddSharedComponent(prefab, new PhysicsWorldIndex{ Value = 0, });
					}
				});
			}

			return entity;
		}

		private BlobAssetReference<Collider> LoadCollider(CharacterTable.CharacterCollider collider) {
			if (_colliders.TryGetValue(collider.id, out BlobAssetReference<Collider> asset)) {
				BlobAssetReference<Collider> capsule = CapsuleCollider.Create(
					geometry: new CapsuleGeometry {
						Radius = collider.radius,
						Vertex0 = new float3(0.0f, collider.radius, 0.0f),
						Vertex1 = new float3(0.0f, collider.height - collider.radius, 0.0f),
					},
					filter: new CollisionFilter {
						BelongsTo = (uint)_characterTable.PhysicsObjectLayer,
						CollidesWith = (uint)_characterTable.PhysicsObjectCollides,
					},
					material: Material.Default
				);
				capsule.Value.SetCollisionResponse(CollisionResponsePolicy.Collide);

				_colliders.TryAdd(collider.id, capsule);
				asset = capsule;
			}
			return asset;
		}
	}
}
