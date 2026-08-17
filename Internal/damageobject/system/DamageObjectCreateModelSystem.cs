using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace hexegeer.internallib {
	/// <summary>
	/// ダメージオブジェクトモデルの読み込み。
	/// Entityを追従する形式であり、生成は遅延して行われても問題ない想定。
	/// </summary>
	[UpdateInGroup(typeof(HexegeerDamageObjectSystemGroup))]
	public partial class DamageObjectCreateModelSystem : SystemBase {
		private EntityQuery _query;

		protected override void OnCreate() {
			base.OnCreate();
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<DamageObjectCreateModelRequest>()
				.Build(EntityManager);
			CheckedStateRef.RequireForUpdate(_query);
		}

		protected override void OnUpdate() {
			NativeArray<DamageObjectCreateModelRequest> requests = _query.ToComponentDataArray<DamageObjectCreateModelRequest>(Allocator.Temp);
			for(int i = 0; i < requests.Length; ++i) {
				Entity observeEntity = requests[i].observeEntity;
				int id = requests[i].id;
				if (DamageObjectModelLookup.TryGetAssetAddress(id, out string address)) {
					Task.Run(async () => await CreateGameObject(id, address, observeEntity));
				}
			}
			requests.Dispose();
			EntityManager.DestroyEntity(_query);
		}

		private async Task CreateGameObject(int id, string address, Entity observeEntity) {
			if (! string.IsNullOrEmpty(address)) {
				GameObject go = await AssetUtil.RequestLoad<GameObject>(address);
				SyncContext.Post(
					() => {
						if (go.TryGetComponent(out HexegeerDamageObjectBehaviour behaviour)) {
							behaviour.OnSpawn(observeEntity, id);
						}
					}
				);
			}
		}
	}
}
