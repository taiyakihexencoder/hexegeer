using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace hexegeer.editor {
	public sealed class HexegeerMasterDataScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
			errorMessages = new List<string>();
			Regex regex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

			List<string> classNames = new List<string>();
			foreach(HexegeerMasterDataSettings.DataClass data in settings.ClassList) {
				if (string.IsNullOrEmpty(data.className)) {
					errorMessages.Add($"Empty name: ID={data.id}");
					continue;
				} else if (!regex.IsMatch(data.className)) {
					errorMessages.Add($"Invalid name: {data.className}");
					continue;
				} else if (classNames.Contains(data.className)) {
					errorMessages.Add($"Duplicated name: {data.className}");
					continue;
				} else {
					classNames.Add(data.className);
				}

				List<string> columnNames = new List<string>();
				foreach(EditorGridView.Column column in data.columns) {
					if (string.IsNullOrEmpty(column.Name)) {
						errorMessages.Add($"Empty column name: {data.className}");
					} else if (!regex.IsMatch(column.Name)) {
						errorMessages.Add($"Invalid column name: {data.className}.{column.Name}");
					} else if (columnNames.Contains(column.Name)) {
						errorMessages.Add($"Duplicated name: {data.className}.{column.Name}");
					} else {
						columnNames.Add(column.Name);
					}
				}
			}

			return errorMessages.Count == 0;
		}
		protected override void WriteScript() {
			HexegeerMasterDataSettings settings = HexegeerMasterDataSettings.instance;
			AppendLine("using Unity.Collections;");
			AppendLine("using Unity.Entities;");

			using(Namespace("hexegeer")) {
				using (Class("MasterDataLoader", isPartial: true, isStatic: true)) {
					foreach (HexegeerMasterDataSettings.DataClass data in settings.ClassList) {
						AppendLine($"private static Entity _table{data.className}Entity = Entity.Null;");
					}

					using (Function("static partial void CreateTableInstance(EntityManager entityManager, MasterDataKey key, byte[] bin)")) {
						AppendLine($"int startOffset = 0;");
						AppendLine($"short columns = System.BitConverter.ToInt16(bin, startOffset);");
						AppendLine($"startOffset += {sizeof(short)};");
						AppendLine($"int dataCount = System.BitConverter.ToInt32(bin, startOffset);");
						AppendLine($"startOffset += {sizeof(int)} + {sizeof(int) + sizeof(byte) + sizeof(float)} * columns + {sizeof(float)} * dataCount;");
						AppendLine($"int offset = startOffset;");
						using (Function("switch (key.Id)")) {
							foreach(HexegeerMasterDataSettings.DataClass data in settings.ClassList) {
								if (TryGetBinaries(data, out byte[] bin)) {
									int offset = 0;
									short columns = System.BitConverter.ToInt16(bin, offset);
									offset += sizeof(short);
									int rows = System.BitConverter.ToInt32(bin, offset);
									offset += sizeof(int);
									int[] columnIds = new int[columns];
									EditorGridView.ColumnType[] columnTypes = new EditorGridView.ColumnType[columns];
									int classSize = 0;
									for(int i = 0; i < columns; ++i) {
										columnIds[i] = System.BitConverter.ToInt32(bin, offset);
										columnTypes[i] = (EditorGridView.ColumnType) (int)bin[offset + sizeof(int)];

										switch(columnTypes[i]) {
											case EditorGridView.ColumnType.INT: { classSize += sizeof(int); break; }
											case EditorGridView.ColumnType.LONG: { classSize += sizeof(long); break; }
											case EditorGridView.ColumnType.BOOL: { classSize += sizeof(bool); break; }
											case EditorGridView.ColumnType.FLOAT: { classSize += sizeof(float); break; }
											case EditorGridView.ColumnType.STRING: { classSize += sizeof(short); break; }
										}

										offset += sizeof(int) + sizeof(byte) + sizeof(float);
									}

									using (Function($"case MasterDataKey.{ToSneak(data.className)}_ID:")) {
										AppendLine($"{data.className}Table componentOf{data.className} = new {data.className}Table();");
										using (Function("using (BlobBuilder builder = new BlobBuilder(Allocator.Temp))")) {
											AppendLine($"ref EventEntityBlobAsset asset = ref builder.ConstructRoot<{data.className}BlobAsset>();");
											AppendLine($"BlobBuilderArray<{data.className}> rows = builder.Allocate(ref asset.rows, dataCount);");

											AppendLine();
											AppendLine($"int textPointer = startOffset + {classSize} * dataCount;");
											for(int i = 0; i < columns; ++i) {
												EditorGridView.Column column = data.columns.Find(_ => _.Id == columnIds[i] && _.Type == columnTypes[i]);
												if (column != null) {
													switch(columnTypes[i]) {
														case EditorGridView.ColumnType.INT: {
															AppendLine($"int[] {column.Name}Array = new int[dataCount];");
															AppendLine($"System.Buffer.BlockCopy(bin, offset, {column.Name}Array, 0, {sizeof(int)} * dataCount);");
															AppendLine($"offset += {sizeof(int)} * dataCount;");
															break;
														}
														case EditorGridView.ColumnType.LONG: {
															AppendLine($"long[] {column.Name}Array = new long[dataCount];");
															AppendLine($"System.Buffer.BlockCopy(bin, offset, {column.Name}Array, 0, {sizeof(long)} * dataCount);");
															AppendLine($"offset += {sizeof(long)} * dataCount;");
															break;
														}
														case EditorGridView.ColumnType.BOOL: {
															AppendLine($"bool[] {column.Name}Array = new bool[dataCount];");
															AppendLine($"System.Buffer.BlockCopy(bin, offset, {column.Name}Array, 0, {sizeof(bool)} * dataCount);");
															AppendLine($"offset += {sizeof(bool)} * dataCount;");
															break;
														}
														case EditorGridView.ColumnType.FLOAT: {
															AppendLine($"float[] {column.Name}Array = new float[dataCount];");
															AppendLine($"System.Buffer.BlockCopy(bin, offset, {column.Name}Array, 0, {sizeof(float)} * dataCount);");
															AppendLine($"offset += {sizeof(float)} * dataCount;");
															break;
														}
														case EditorGridView.ColumnType.STRING: {
															AppendLine($"short[] {column.Name}LengthArray = new short[dataCount];");
															AppendLine($"System.Buffer.BlockCopy(bin, offset, {column.Name}LengthArray, 0, {sizeof(short)} * dataCount);");
															AppendLine($"offset += {sizeof(short)} * dataCount;");
															AppendLine($"string[] {column.Name}Array = new string[dataCount];");
															using (Function("for (int i = 0; i < dataCount; ++i)")) {
																AppendLine($"{column.Name}Array[i] = System.Text.Encoding.UTF8.GetString(bin, textPointer, {column.Name}LengthArray[i]);");
																AppendLine($"textPointer += {column.Name}LengthArray[i];");
															}
															break;
														}
													}
												}
											}
											AppendLine($"");

											using (Function($"for (int i = 0; i < dataCount; ++i)")) {
												AppendLine($"rows[i] = new {data.className} {{");
												using (Indent) {
													for(int i = 0; i < columns; ++i) {
														EditorGridView.Column column = data.columns.Find(_ => _.Id == columnIds[i] && _.Type == columnTypes[i]);
														if (column != null) {
															if (column.Type == EditorGridView.ColumnType.STRING) {
																AppendLine($"{column.Name} = new FixedString64Bytes({column.Name}Array[i]),");
															} else {
																AppendLine($"{column.Name} = {column.Name}Array[i],");
															}
														}
													}
												}
												AppendLine($"}};");
											}
											AppendLine($"componentOf{data.className}.reference = builder.CreateBlobAssetReference<{data.className}BlobAsset>(Allocator.Persistent);");
											AppendLine($"_table{data.className}Entity = entityManager.Create(componentOf{data.className});");
										}
										AppendLine("break;");
									}

								}
							}
						}
					}

					AppendLine();

					using(Function("static partial void DisposeTable(EntityManager entityManager, MasterDataKey key)")) {
						foreach (HexegeerMasterDataSettings.DataClass data in settings.ClassList) {
							using (Function("switch (key.Id)")) {
								using (Function($"case MasterDataKey.{ToSneak(data.className)}_ID:")) {
									using (Function($"if (entityManager.Exists(_table{data.className}Entity))")) {
										AppendLine($"entityManager.GetComponentData<{data.className}Table>(_table{data.className}Entity).reference.Dispose();");
										AppendLine($"entityManager.DestroyEntity(_table{data.className}Entity);");
										AppendLine($"_table{data.className}Entity = Entity.Null;");
									}
									AppendLine($"break;");
								}
							}
						}
					}

					AppendLine();
					
					using (Function("static partial void DisposeAllTable(EntityManager entityManager)")) {
						foreach (HexegeerMasterDataSettings.DataClass data in settings.ClassList) {
							using(Function($"if (entityManager.Exists(_table{data.className}Entity))")){
								AppendLine($"DisposeTable(entityManager, MasterDataKey.{data.className});");
							}
						}
					}
				}

				AppendLine();

				using (Struct("MasterDataKey", isPartial: true)) {
					List<string> constName = new List<string>();
					List<string> keyName = new List<string>();
					List<string> fileName = new List<string>();
					foreach(HexegeerMasterDataSettings.DataClass data in settings.ClassList) {
						string str = ToSneak(data.className);
						AppendLine($"public const int {str}_ID = {data.id};");
						keyName.Add(data.className);
						constName.Add(str);
						fileName.Add(data.fileName);
					}

					for (int i = 0; i < settings.ClassList.Count; ++i) {
						AppendLine($"public static MasterDataKey {keyName[i]} = new MasterDataKey({constName[i]}_ID, \"{keyName[i]}\", \"{fileName[i]}\");");
					}
				}

				AppendLine();

				foreach(HexegeerMasterDataSettings.DataClass data in settings.ClassList) {
					using (Struct($"{data.className}Table : IComponentData")) {
						AppendLine($"public BlobAssetReference<{data.className}BlobAsset> reference;");
					}

					AppendLine();

					using (Struct($"{data.className}BlobAsset")) {
						AppendLine($"public BlobArray<{data.className}> rows;");
					}

					AppendLine();

					using (Struct(data.className)) {
						foreach(EditorGridView.Column column in data.columns) {
							switch(column.Type) {
								case EditorGridView.ColumnType.INT: {
									AppendLine($"public int {column.Name};");
									break;
								}
								case EditorGridView.ColumnType.LONG: {
									AppendLine($"public long {column.Name};");
									break;
								}
								case EditorGridView.ColumnType.BOOL: {
									AppendLine($"public bool {column.Name};");
									break;
								}
								case EditorGridView.ColumnType.FLOAT: {
									AppendLine($"public float {column.Name};");
									break;
								}
								case EditorGridView.ColumnType.STRING: {
									AppendLine($"public FixedString64Bytes {column.Name};");
									break;
								}
							}
						}
					}
					AppendLine();
				}
			}
		}

		private string ToSneak(string pattern) {
			string str = "";
			foreach (char ch in pattern) {
				if (str.Length == 0) {
					str += ch;
				} else {
					if ('A' <= ch && ch <= 'Z') {
						str += $"_{ch}";
					} else if ('a' <= ch && ch <= 'z'){
						str += (char)(ch - 'a' + 'A');
					} else {
						str += ch;
					}
				}
			}
			return str;
		}

		private bool TryGetBinaries(HexegeerMasterDataSettings.DataClass data, out byte[] bin) {
			string path = $"{Application.dataPath}{Path.DirectorySeparatorChar}{HexegeerMasterDataTable.TablePath}{data.fileName}";
			if (File.Exists(path)) {
				try {
					bin = File.ReadAllBytes(path);
					return true;
				} catch (System.Exception) {
					bin = null;
					return false;
				}
			} else {
				bin = null;
				return false;
			}
		}
	}
}