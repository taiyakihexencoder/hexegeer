using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace hexegeer.internallib {
	/// <summary>
	/// キャラクターモデルの読み込み。
	/// Entityを追従する形式であり、生成は遅延して行われても問題ない想定。
	/// </summary>
	[UpdateInGroup(typeof(HexegeerCharacterSystemGroup))]
	public partial class CharacterCreateModelSystem : SystemBase {
		private EntityQuery _query;

		private Dictionary<int, string> _addressTable;

		protected override void OnCreate() {
			base.OnCreate();
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterCreateModelRequest>()
				.Build(EntityManager);
			CheckedStateRef.RequireForUpdate(_query);

			_addressTable = new Dictionary<int, string>();
		}

		protected override void OnStartRunning() {
			base.OnStartRunning();
			// 読み込みテーブル作成
			if (_addressTable.Count == 0) {
				CharacterBlobTable characterTable = SystemAPI.GetSingleton<CharacterBlobTable>();
				for (int i = 0; i < characterTable.character.Value.rows.Length; ++i) {
					_addressTable.Add(characterTable.character.Value.rows[i].id, characterTable.character.Value.rows[i].modelAsset.ConvertToString());
				}
			}
		}

		protected override void OnUpdate() {
			NativeArray<CharacterCreateModelRequest> requests = _query.ToComponentDataArray<CharacterCreateModelRequest>(Allocator.Temp);
			for(int i = 0; i < requests.Length; ++i) {
				Entity observeEntity = requests[i].observeEntity;
				int id = requests[i].id;
				if (_addressTable.TryGetValue(id, out string address) && address.Length > 0) {
					Task.Run(async () => await CreateGameObject(address, observeEntity));
				}
			}
			requests.Dispose();
			EntityManager.DestroyEntity(_query);
		}

		private async Task CreateGameObject(string address, Entity observeEntity) {
			GameObject go = await AssetUtil.RequestLoad<GameObject>(address);
			SyncContext.Post(
				() => {
					if (go.TryGetComponent(out HexegeerCharacterBehaviour behaviour)) {
						behaviour.OnSpawn(observeEntity, address);
					}
				}
			);
		}
	}

}
