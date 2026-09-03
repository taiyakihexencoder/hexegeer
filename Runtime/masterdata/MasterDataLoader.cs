using System.IO;
using System.Threading.Tasks;
using hexegeer.internallib;
using Unity.Entities;
using UnityEngine;

namespace hexegeer {
	public static partial class MasterDataLoader {
		private static string dataPath => $"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}hexegeer{Path.DirectorySeparatorChar}data";

		public static async Task Load(MasterDataKey key) {
			string path = SyncContext.Send(() => $"{dataPath}{Path.DirectorySeparatorChar}{key.FileName}");

			try {
				byte[] bin = await File.ReadAllBytesAsync(path);
				SyncContext.Post(() => {
					EntityManager entityManager = ECS.EntityManager;
					CreateTableInstance(entityManager, key, bin);
				});
			} catch (System.Exception e) {
				D.LogE(e);
			}
		}

		public static void Unload(MasterDataKey key) {
			EntityManager entityManager = ECS.EntityManager;
			DisposeTable(entityManager, key);
		}

		public static void DisposeAllTable() {
			EntityManager entityManager = ECS.EntityManager;
			DisposeAllTable(entityManager);
		}

		static partial void CreateTableInstance(EntityManager entityManager, MasterDataKey key, byte[] bin);

		static partial void DisposeTable(EntityManager entityManager, MasterDataKey key);
		static partial void DisposeAllTable(EntityManager entityManager);
	}
}