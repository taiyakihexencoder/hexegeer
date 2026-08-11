using System.Collections.Generic;
using System.Net;
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

		protected override void OnCreate() {
			base.OnCreate();
			_query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterCreateModelRequest>()
				.Build(EntityManager);
			CheckedStateRef.RequireForUpdate(_query);
		}

		protected override void OnUpdate() {
			NativeArray<CharacterCreateModelRequest> requests = _query.ToComponentDataArray<CharacterCreateModelRequest>(Allocator.Temp);
			for(int i = 0; i < requests.Length; ++i) {
				Entity observeEntity = requests[i].observeEntity;
				int id = requests[i].id;
				if (CharacterModelLookup.TryGetProfile(id, out CharacterTable.ModelProfile profile)) {
					Task.Run(async () => await CreateGameObject(id, profile, observeEntity));
				}
			}
			requests.Dispose();
			EntityManager.DestroyEntity(_query);
		}

		private async Task CreateGameObject(int id, CharacterTable.ModelProfile profile, Entity observeEntity) {
			if (! string.IsNullOrEmpty(profile.modelAsset)) {
				AnimationClip[] overrideClips = new AnimationClip[profile.overrideAnimations.Count];
				for (int i = 0; i < overrideClips.Length; ++i) {
					overrideClips[i] = await AssetUtil.RequestLoad<AnimationClip>(profile.overrideAnimations[i]);
				}

				AnimationClip[] additiveClips = new AnimationClip[profile.additiveAnimations.Count];
				for (int i = 0; i < additiveClips.Length; ++i) {
					additiveClips[i] = await AssetUtil.RequestLoad<AnimationClip>(profile.additiveAnimations[i]);
				}

				AnimationClip[] baseClips = new AnimationClip[profile.baseAnimations.Count];
				for (int i = 0; i < baseClips.Length; ++i) {
					baseClips[i] = await AssetUtil.RequestLoad<AnimationClip>(profile.baseAnimations[i]);
				}

				GameObject go = await AssetUtil.RequestLoad<GameObject>(profile.modelAsset);
				SyncContext.Post(
					() => {
						if (go.TryGetComponent(out HexegeerCharacterBehaviour behaviour)) {
							behaviour.OnSpawn(
								observeEntity, 
								id,
								overrideClips,
								additiveClips,
								baseClips
							);
						}
					}
				);

			}
		}
	}

}
