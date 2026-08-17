using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerWorldSystemGroup))]
	public partial class HexegeerWorldSystem : SystemBase {
		private Entity _masterDataEntity;
		private Entity _fieldTableEntity;
		private Entity _layoutTableEntity;
		private Entity _characterTableEntity;
		private Entity _damageObjectTableEntity;
		private Entity _cameraEntity;
		private Entity _worldReadyEntity;

		private Entity _physicsStepEntity;

		protected override void OnCreate() {
			base.OnCreate();
			_masterDataEntity = Entity.Null;
			_fieldTableEntity = Entity.Null;
			_layoutTableEntity = Entity.Null;
			_characterTableEntity = Entity.Null;
			_damageObjectTableEntity = Entity.Null;
			_cameraEntity = Entity.Null;
			_worldReadyEntity = Entity.Null;

			_physicsStepEntity = Entity.Null;
		}

		protected override void OnStartRunning() {
			base.OnStartRunning();

			CreateDebugView(EntityManager);
			CreateCameraInstance(EntityManager);

			_masterDataEntity = EntityManager.Create(
				new Parent(),
				new LocalToWorld{ Value = float4x4.identity, },
				LocalTransform.Identity,
				new AttachHexegeerTree()
			);
			ECS.SetEntityName(EntityManager, _masterDataEntity, "Master Data@Hexegeer");

			// Dummy Entry Point
			HexegeerManager.CreateEntryPoint(EntityManager, new float3(0f, 20f, 0f));

			// Global Content
			CreateGlobalContentKeyRequest(EntityManager);

			Task.Run(LoadTask);
		}

		private async Task LoadTask() {
			// Field Setting
			await LoadFieldTable();

			// Layout Settings
			await LoadLayoutTable();

			// Character Setting
			await LoadCharacterTable();

			// Damage Object Setting
			await LoadDamageObjectTable();

			await Task.Yield();

			SyncContext.Send(() => {
				_physicsStepEntity = EntityManager.Create(
					new PhysicsStep {
						SimulationType = SimulationType.UnityPhysics,
						Gravity = new float3(0.0f, -9.81f, 0.0f),
						SolverIterationCount = 4,
						SubstepCount = 1,
						MultiThreaded = 1,
					}
				);
				ECS.SetEntityName(EntityManager, _physicsStepEntity, "Physics Step");

				_worldReadyEntity = EntityManager.Create(
					new Parent(),
					new LocalToWorld{ Value = float4x4.identity, },
					LocalTransform.Identity,
					new AttachHexegeerTree(),
					new HexegeerWorldReady{ }
				);
				ECS.SetEntityName(EntityManager, _masterDataEntity, "World Ready@Hexegeer");
			});
		}

		private void CreateDebugView(EntityManager entityManager) {
			Entity debugViewEntity = entityManager.Create(
				new PhysicsDebugDisplayData {
					DrawColliders = 0,
					DrawColliderEdges = 1,
				},
				new Parent(),
				new AttachHexegeerTree(),
				LocalTransform.Identity,
				new LocalToWorld{ Value = float4x4.identity, }
			);
			ECS.SetEntityName(entityManager, debugViewEntity, "Physics Debug Display@Hexegeer");
		}

		private void CreateCameraInstance(EntityManager entityManager) {
			_cameraEntity = entityManager.Create(
				new CameraInstance(),
				new CameraBounds(),
				new FixedCamera(),
				new FollowCamera(),
				new CameraLerp(),
				LocalTransform.Identity,
				new LocalToWorld { Value = float4x4.identity, },
				new Parent(),
				new AttachHexegeerTree()
			);

			entityManager.SetComponentEnabled<CameraBounds>(_cameraEntity, false);
			entityManager.SetComponentEnabled<FixedCamera>(_cameraEntity, false);
			entityManager.SetComponentEnabled<FollowCamera>(_cameraEntity, false);
			entityManager.SetComponentEnabled<CameraLerp>(_cameraEntity, false);
			ECS.SetEntityName(entityManager, _cameraEntity, "Camera@Hexegeer");
		}

		/// <summary>
		/// FieldTable -> FieldBlobTableシングルトン
		/// </summary>
		/// <returns></returns>
		private async Task LoadFieldTable() {
			// FieldSettingの読み込み
			SyncContext.Post(() => {
				FieldSettingGenerator.Generate(EntityManager, _masterDataEntity);
			});

			// FieldTableの読み込み
			FieldTable table = await AssetUtil.RequestLoad<FieldTable>(FieldTable.RESOURCE_ADDRESS);

			SyncContext.Post(() => {
				using (BlobBuilder builder = new BlobBuilder(Allocator.Temp)) {
					ref FieldBlobAsset asset = ref builder.ConstructRoot<FieldBlobAsset>();
					BlobBuilderArray<FieldInfo> rows = builder.Allocate(ref asset.rows, table.Rows.Length);
					for(int i = 0; i < table.Rows.Length; ++i) {
						FieldTable.Row row = table.Rows[i];
						rows[i] = new FieldInfo {
							id = row.id,
							contentKey = row.contentKey,
							address = row.address,
							name = row.name,
							guid = row.guid,
							position = row.position,
							rotation = row.rotation,
							boundsMin = row.boundsMin,
							boundsMax = row.boundsMax,
						};
					}
					FieldBlobTable blobTable = new FieldBlobTable {
						asset = builder.CreateBlobAssetReference<FieldBlobAsset>(Allocator.Persistent),
					};

					_fieldTableEntity = EntityManager.Create(
						blobTable,
						new Parent{ Value = _masterDataEntity, },
						new LocalToWorld{ Value = float4x4.identity, },
						LocalTransform.Identity
					);
					ECS.SetEntityName(EntityManager, _fieldTableEntity, "Field Table@Hexegeer");
				}
				AssetUtil.Release(FieldTable.RESOURCE_ADDRESS);
			});


		}

		private async Task LoadLayoutTable() {
			LayoutTable table = await AssetUtil.RequestLoad<LayoutTable>(LayoutTable.RESOURCE_ADDRESS);
			SyncContext.Post(() => {
				using (BlobBuilder layoutBuilder = new BlobBuilder(Allocator.Temp)) {
					ref LayoutBlobAsset asset = ref layoutBuilder.ConstructRoot<LayoutBlobAsset>();
					BlobBuilderArray<LayoutProfile> rows = layoutBuilder.Allocate(ref asset.rows, table.LayoutProfiles.Count);
					for (int i = 0; i < table.LayoutProfiles.Count; ++i) {
						LayoutTable.Profile profile = table.LayoutProfiles[i];

						rows[i].contentKey = profile.ContentKey;

						BlobBuilderArray<LayoutLoadCharacterInfo> loadCharacters = layoutBuilder.Allocate(ref rows[i].loadCharacters, profile.CharacterIds.Count);
						for (int j = 0; j < profile.CharacterIds.Count; ++j) {
							loadCharacters[j] = new LayoutLoadCharacterInfo {
								id = profile.CharacterIds[j],
							};
						}

						BlobBuilderArray<LayoutCharacterInfo> characterLayout = layoutBuilder.Allocate(ref rows[i].characterLayout, profile.Characters.Count);
						for (int j = 0; j < profile.Characters.Count; ++j) {
							characterLayout[j] = new LayoutCharacterInfo {
								id = profile.Characters[j].Id,
								position = profile.Characters[j].Position,
								rotation = profile.Characters[j].Rotation,
							};
						}
					}

					LayoutBlobTable component = new LayoutBlobTable {
						asset = layoutBuilder.CreateBlobAssetReference<LayoutBlobAsset>(Allocator.Persistent),
					};
					
					_layoutTableEntity = EntityManager.Create(
						component,
						new Parent { Value = _masterDataEntity, },
						new LocalToWorld{ Value = float4x4.identity, },
						LocalTransform.Identity
					);
					ECS.SetEntityName(EntityManager, _layoutTableEntity, "Layout Table@Hexegeer");
				}

				AssetUtil.Release(LayoutTable.RESOURCE_ADDRESS);
			});
		}

		private async Task LoadCharacterTable() {
			CharacterTable table = await AssetUtil.RequestLoad<CharacterTable>(CharacterTable.RESOURCE_ADDRESS);
			SyncContext.Post(() => {
				CharacterBlobTable blobTable = new CharacterBlobTable { };

				blobTable.physicsObjectLayer = table.PhysicsObjectLayer;
				blobTable.physicsObjectCollides = table.PhysicsObjectCollides;

				// Master
				using (BlobBuilder characterBuilder = new BlobBuilder(Allocator.Temp)) {
					ref CharacterBlobAsset asset = ref characterBuilder.ConstructRoot<CharacterBlobAsset>();
					BlobBuilderArray<CharacterInfo> rows = characterBuilder.Allocate(ref asset.rows, table.Characters.Count);
					for (int i = 0; i < table.Characters.Count; ++i) {
						CharacterTable.Character row = table.Characters[i];
						CharacterModelLookup.Register(row);

						rows[i] = new CharacterInfo {
							id = row.id,
							name = row.name,
							collider = row.collider,
							belongsTo = row.belongsTo,
							collidesWith = row.collidesWith,
							hasObservationPoint = row.hasObservationPoint,
						};
					}
					
					BlobBuilderArray<CharacterHitAreaListAsset> hitArea = characterBuilder.Allocate(ref asset.hitArea, table.Characters.Count);
					for (int i = 0; i < table.Characters.Count; ++i) {
						BlobBuilderArray<CharacterHitArea> area = characterBuilder.Allocate(ref hitArea[i].list, table.Characters[i].hitAreas.Count);
						for (int j = 0; j < table.Characters[i].hitAreas.Count; ++j) {
							CharacterTable.HitArea areaInfo = table.Characters[i].hitAreas[j];
							area[j] = new CharacterHitArea {
								shape = areaInfo.shape,
								extent = areaInfo.extent,
								position = areaInfo.position,
								rotation = areaInfo.rotation,
							};
						}
					}

					blobTable.character = characterBuilder.CreateBlobAssetReference<CharacterBlobAsset>(Allocator.Persistent);

				}

				// Collider
				using (BlobBuilder colliderBuilder = new BlobBuilder(Allocator.Temp)) {
					ref CharacterColliderBlobAsset asset = ref colliderBuilder.ConstructRoot<CharacterColliderBlobAsset>();
					BlobBuilderArray<CharacterColliderInfo> rows = colliderBuilder.Allocate(ref asset.rows, table.Colliders.Count);
					for (int i = 0; i < table.Colliders.Count; ++i) {
						CharacterTable.CharacterCollider row = table.Colliders[i];
						rows[i] = new CharacterColliderInfo {
							id = row.id,
							name = row.name,
							radius = row.radius,
							height = row.height,
						};
					}
					blobTable.collider = colliderBuilder.CreateBlobAssetReference<CharacterColliderBlobAsset>(Allocator.Persistent);
				}

				// KeyTable
				using (BlobBuilder loadTableBuilder = new BlobBuilder(Allocator.Temp)) {
					ref CharacterLoadTableBlobAsset tableAsset = ref loadTableBuilder.ConstructRoot<CharacterLoadTableBlobAsset>();
					BlobBuilderArray<CharacterLoadListAsset> rows = loadTableBuilder.Allocate(ref tableAsset.rows, table.KeyTables.Length);
					for (int i = 0; i < table.KeyTables.Length; ++i) {
						CharacterTable.KeyTable keyTable = table.KeyTables[i];
						BlobBuilderArray<CharacterLoadElement> list = loadTableBuilder.Allocate(ref rows[i].list, keyTable.characterIndices.Count);
						rows[i].key = keyTable.key;
						for (int j = 0; j < keyTable.characterIndices.Count; ++j) {
							list[j] = new CharacterLoadElement { index = keyTable.characterIndices[j], };
						}
					}
					blobTable.loadTable = loadTableBuilder.CreateBlobAssetReference<CharacterLoadTableBlobAsset>(Allocator.Persistent);
				}

				_characterTableEntity = EntityManager.Create(
					blobTable,
					new Parent { Value = _masterDataEntity, },
					new LocalToWorld{ Value = float4x4.identity, },
					LocalTransform.Identity
				);
				ECS.SetEntityName(EntityManager, _characterTableEntity, "Character Table@Hexegeer");

				AssetUtil.Release(CharacterTable.RESOURCE_ADDRESS);
			});
		}

		private async Task LoadDamageObjectTable() {
			DamageObjectTable table = await AssetUtil.RequestLoad<DamageObjectTable>(DamageObjectTable.RESOURCE_ADDRESS);
			SyncContext.Post(() => {
				DamageObjectBlobTable blobTable = new DamageObjectBlobTable();

				using (BlobBuilder builder = new BlobBuilder(Allocator.Temp)) {
					ref DamageObjectBlobAsset damageObject = ref builder.ConstructRoot<DamageObjectBlobAsset>();
					BlobBuilderArray<DamageObjectInfo> objectList = builder.Allocate(ref damageObject.objectList, table.DamageObjects.Count);
					for (int i = 0; i < table.DamageObjects.Count; ++i) {
						DamageObjectTable.DamageObject row = table.DamageObjects[i];
						DamageObjectModelLookup.Register(row);

						objectList[i] = new DamageObjectInfo {
							id = row.id,
							name = new FixedString64Bytes(row.name),
							collider = row.collider,
						};
					}

					BlobBuilderArray<DamageObjectColliderInfo> colliderList = builder.Allocate(ref damageObject.colliderList, table.Colliders.Count);
					for (int i = 0; i < table.Colliders.Count; ++i) {
						DamageObjectTable.DamageObjectCollider collider = table.Colliders[i];
						colliderList[i] = new DamageObjectColliderInfo {
							id = collider.id,
							collidesWith = collider.collidesWith,
							belongsTo = collider.belongsTo,
							extent = collider.extent,
							rotation = collider.rotation,
							shape = collider.shape,
						};
					}
					blobTable.damageObject = builder.CreateBlobAssetReference<DamageObjectBlobAsset>(Allocator.Persistent);
				}

				using (BlobBuilder builder = new BlobBuilder(Allocator.Temp)) {
					ref DamageObjectKeyListBlobAsset keyTable = ref builder.ConstructRoot<DamageObjectKeyListBlobAsset>();
					BlobBuilderArray<DamageObjectKeyList> damageObjectKeyList = builder.Allocate(ref keyTable.list, table.ContentKeyTable.Length);
					for (int i = 0; i < table.ContentKeyTable.Length; ++i) {
						DamageObjectTable.KeyTable damageObjectKeyTable = table.ContentKeyTable[i];
						damageObjectKeyList[i] = new DamageObjectKeyList {
							key = damageObjectKeyTable.key,
						};

						BlobBuilderArray<DamageObjectLoadElement> elements = builder.Allocate(ref damageObjectKeyList[i].elements, table.ContentKeyTable[i].indices.Count);
						for (int j = 0; j < table.ContentKeyTable[i].indices.Count; ++j) {
							elements[j] = new DamageObjectLoadElement {
								index = damageObjectKeyTable.indices[j],
							};
						}
					}
					blobTable.keyTable = builder.CreateBlobAssetReference<DamageObjectKeyListBlobAsset>(Allocator.Persistent);
				}

				_damageObjectTableEntity = EntityManager.Create(
					blobTable,
					new Parent{ Value = _masterDataEntity, },
					new LocalToWorld{ Value = float4x4.identity, },
					LocalTransform.Identity
				);
				ECS.SetEntityName(EntityManager, _damageObjectTableEntity, "Damage Object Table@Hexegeer");

				AssetUtil.Release(DamageObjectTable.RESOURCE_ADDRESS);
			});
		}

		private void CreateGlobalContentKeyRequest(EntityManager entityManager) {
			// FieldLoadingSystemを起動してFieldHeaderを配置するために、
			// ダミーのリクエストを投げる
			entityManager.Create(new FieldLoadRequest { id = 0, });

			// ContentKeyの読み込みリクエストも投げておく
			entityManager.Create(new ContentKeyLoadRequest{ contentKey = ContentKey.Global.value, });
		}

		protected override void OnStopRunning() {
			if (_worldReadyEntity != Entity.Null) {
				EntityManager.DestroyEntity(_worldReadyEntity);
				_worldReadyEntity = Entity.Null;
			}

			// Field Setting
			if (_fieldTableEntity != Entity.Null) {
				FieldBlobTable table = EntityManager.GetComponentData<FieldBlobTable>(_fieldTableEntity);
				table.asset.Dispose();
				EntityManager.DestroyEntity(_fieldTableEntity);
				_fieldTableEntity = Entity.Null;
			}

			if (_layoutTableEntity != Entity.Null) {
				LayoutBlobTable table = EntityManager.GetComponentData<LayoutBlobTable>(_layoutTableEntity);
				table.asset.Dispose();
				EntityManager.DestroyEntity(_layoutTableEntity);
				_layoutTableEntity = Entity.Null;
			}
			
			// Character Setting
			if (_characterTableEntity != Entity.Null) {
				CharacterBlobTable table = EntityManager.GetComponentData<CharacterBlobTable>(_characterTableEntity);
				table.character.Dispose();
				table.collider.Dispose();
				table.loadTable.Dispose();
				EntityManager.DestroyEntity(_characterTableEntity);
				_characterTableEntity = Entity.Null;
			}

			if (_damageObjectTableEntity != Entity.Null) {
				DamageObjectBlobTable table = EntityManager.GetComponentData<DamageObjectBlobTable>(_damageObjectTableEntity);
				table.damageObject.Dispose();
				table.keyTable.Dispose();
				EntityManager.DestroyEntity(_damageObjectTableEntity);
				_damageObjectTableEntity = Entity.Null;
			}

			if (SystemAPI.TryGetSingletonEntity<FieldSetting>(out Entity singleton)) {
				EntityManager.DestroyEntity(singleton);
			}

			if (_masterDataEntity != Entity.Null) {
				EntityManager.DestroyEntity(_masterDataEntity);
				_masterDataEntity = Entity.Null;
			}
			base.OnStopRunning();
		}

		protected override void OnUpdate() {
			
		}
	}
}