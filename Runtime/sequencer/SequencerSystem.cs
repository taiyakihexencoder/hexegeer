using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	[UpdateInGroup(typeof(internallib.HexegeerSimulationSystemGroup))]
	public partial class SequencerSystem : SystemBase {
		private EntityQuery query;

		protected override void OnCreate() {
			query = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<SequenceRequest>()
				.Build(EntityManager);
		}

		protected override void OnUpdate() {
			Dictionary<int, SequencerContext> contextList = SequencerContext.Contexts;

			if (!query.IsEmpty) {
				NativeArray<SequenceRequest> requests = query.ToComponentDataArray<SequenceRequest>(Allocator.Temp);

				foreach(SequenceRequest request in requests) {
					if (contextList.TryGetValue(request.contextKey, out SequencerContext context)) {
						context.RequestSequence(request.sequenceId);
					}
				}

				requests.Dispose();
				EntityManager.DestroyEntity(query);
			}

			foreach(SequencerContext context in contextList.Values) {
				context.OnUpdate();
			}
		}
	}
}