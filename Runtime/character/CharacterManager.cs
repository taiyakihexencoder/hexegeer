using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace hexegeer {
	public static class CharacterManager {
		private static EntityQuery _characterQuery;
		private static EntityQuery _requestQuery;

		[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() {
			_characterQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterHeader>()
				.Build(ECS.EntityManager);
			_requestQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterSpawnRequest>()
				.Build(ECS.EntityManager);
		}

		public static void RequestSpawn(
			EntityManager entityManager,
			CharacterId characterId, 
			float3 position, 
			quaternion rotation
		) {
			entityManager.Create(
				new CharacterSpawnRequest {
					id = characterId.Id,
					position = position,
					rotation = rotation,
				}
			);
		}

		public static async Task<Entity> WaitEntitySpawn(CharacterId characterId) {
			Entity target = Entity.Null;
			bool continueWait = true;
			while(target == Entity.Null && continueWait) {
				SyncContext.Post(() => {
					if (! ECS.valid) {
						// エディタ用。起動時にエラーが起きるとループを抜け出せなくなるため。
						continueWait = false;
					} else {
						EntityManager entityManager = ECS.EntityManager;
						NativeArray<Entity> entities = _characterQuery.ToEntityArray(Allocator.Temp);
						foreach(Entity entity in entities) {
							CharacterHeader header = entityManager.GetComponentData<CharacterHeader>(entity);
							if (header.id == characterId.Id) {
								target = entity;
								break;
							}
						}
						entities.Dispose();

						// もしリクエストがなければ待っても仕方ないので中断する
						if (continueWait && _requestQuery.IsEmpty) {
							continueWait = false;
						}
					}
				});
				await Task.Delay(50);

			}

			if (target != Entity.Null) {
				// ビジュアルの作成
				SyncContext.Post(() => {
					EntityManager entityManager = ECS.EntityManager;
					entityManager.Create(
						new CharacterCreateModelRequest {
							id = characterId.Id,
							observeEntity = target,
						}
					);
				});
			}

			return target;
		}
	}
}