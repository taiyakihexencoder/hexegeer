using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;

namespace hexegeer {
	/// <summary>
	/// フィールドの読み込み
	/// </summary>
	[UpdateInGroup(typeof(HexegeerFieldModuleSystemGroup))]
	public partial class FieldLoadingSystem : SystemBase {
		private EntityQuery _headerQuery;
		private EntityQuery _requestQuery;

		protected override void OnCreate() {
			_headerQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<FieldHeader>()
				.WithNone<FieldUnloadExclude>()
				.Build(EntityManager);

			_requestQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<FieldLoadRequest>()
				.Build(EntityManager);

			RequireForUpdate<FieldBlobTable>();
		}

		protected override void OnDestroy() { }

		protected override void OnUpdate() {
			FieldBlobTable table = SystemAPI.GetSingleton<FieldBlobTable>();
			NativeArray<Entity> headerEntities = _headerQuery.ToEntityArray(Allocator.Temp);
			NativeArray<FieldHeader> headerComponents = _headerQuery.ToComponentDataArray<FieldHeader>(Allocator.Temp);
			NativeArray<FieldLoadRequest> requests = _requestQuery.ToComponentDataArray<FieldLoadRequest>(Allocator.Temp);

			foreach(FieldLoadRequest request in requests) {
				int id = request.id;
				if (TrySearchHeader(id, headerEntities, headerComponents, out Entity header)) {
					HexegeerFieldModule.OnRequestCreateFieldEntity(table, header, id, request.keep);
				} else {
					D.LogW($"Not found header: id={id}");
				}
			}
			requests.Dispose();
			headerEntities.Dispose();
			headerComponents.Dispose();

			// リクエストを破棄
			EntityManager.DestroyEntity(_requestQuery);
		}

		private bool TrySearchHeader(int id, in NativeArray<Entity> entities, in NativeArray<FieldHeader> headers, out Entity entity) {
			for(int i = 0; i < headers.Length; ++i) {
				if (headers[i].id == id) {
					entity = entities[i];
					return true;
				}
			}
			entity = Entity.Null;
			return false;
		}
	}
}