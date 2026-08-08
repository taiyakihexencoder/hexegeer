using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace hexegeer.internallib {
	public static class PersistentData {
		private static string rootPath;

		private static char separator => Path.DirectorySeparatorChar;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Init() {
			rootPath = Application.persistentDataPath;
		}

		public async static Task Save<T>(string path, T data, ISerializer<T> serializer, System.Action<System.Exception> callback = null) {
			byte[] raw = SyncContext.Send(() => serializer.Serialize(data));
			System.Exception error = null;

			await Task.Run(() => {
				try {
					string absPath = $"{rootPath}{separator}{path}"; 

					using (FileStream stream = new FileStream(absPath, FileMode.OpenOrCreate, FileAccess.Write)) {
						using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8)) {
							writer.Write(raw.Length);
							writer.Write(raw);
						}
					}
				} catch (System.Exception e) {
					error = e;
				}
			});

			callback?.Invoke(error);
		}

		public async static Task Load<T>(string path, IDeserializer<T> deserializer, System.Action<T, System.Exception> callback) {
			System.Exception error = null;
			byte[] raw = await Task.Run(() => {
				try {
					string absPath = $"{rootPath}{separator}{path}";

					using (FileStream stream = new FileStream(absPath, FileMode.Open, FileAccess.Read)) {
						if (stream.Length < 4) {
							throw new System.Exception("Empty File.");
						}

						using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8)) {
							int size = reader.ReadInt32();
							return reader.ReadBytes(size);
						}
					}
				} catch (System.Exception e) {
					error = e;
					return null;
				}
			});

			SyncContext.Send(() => {
				if (raw == null) {
					callback(default, error);
				} else {
					callback(deserializer.Deserialize(raw), error);
				}
			});
		}

		public interface ISerializer<T> {
			byte[] Serialize(in T data);
		}

		public interface IDeserializer<T> {
			T Deserialize(in byte[] raw);
		}
	}
}