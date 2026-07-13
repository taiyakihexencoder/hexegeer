using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace hexegeer.editor {
	internal sealed class FieldScriptGenerator : SourceCodeGenerator {
		public override bool Validation(out List<string> errorMessages) {
			errorMessages = new List<string>();

			FieldMainSettings mainSettings = FieldMainSettings.instance;
			System.Type resourceType = mainSettings.ViewType.GetResourceType();

			Regex invalidAddressRegex = new Regex(@"/$");
			Regex invalidFileNameRegex = new Regex(@"[^a-zA-Z0-9_]");
			foreach(string guid in AssetDatabase.FindAssets($"t:{resourceType.Name}")) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				BaseFieldBlueprint blueprint = AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath);

				if (string.IsNullOrEmpty(blueprint.RuntimeAssetAddress)) {
					// addressが空
					errorMessages.Add("Empty address:" + blueprint.name);
				} else if (blueprint.RuntimeAssetAddress.Length > 62) {
					// FixedString64Bytesの限界値
					errorMessages.Add("Too long address:" + blueprint.RuntimeAssetAddress);
				} else if(invalidAddressRegex.IsMatch(blueprint.RuntimeAssetAddress)) {
					errorMessages.Add("Invalid address:" + blueprint.RuntimeAssetAddress);
				} else if(invalidFileNameRegex.IsMatch(blueprint.name)) {
					errorMessages.Add("Invalid asset name:" + blueprint.name);
				}
			}

			return errorMessages.Count == 0;
		}

		protected override void WriteScript() {
			FieldMainSettings mainSettings = FieldMainSettings.instance;
			System.Type resourceType = mainSettings.ViewType.GetResourceType();
			
			string[] guids = AssetDatabase.FindAssets($"t:{resourceType.Name}");

			AppendLine("using Unity.Entities;");
			AppendLine("using Unity.Mathematics;");
			AppendLine("using Unity.Transforms;");
			AppendLine("using hexegeer.internallib;");

			AppendLine();

			using (Namespace("hexegeer")) {
				using (Struct("FieldAssetAddress", isPartial: true, isStatic: false, isReadonly: true)) {
					foreach (string guid in guids) {
						string assetPath = AssetDatabase.GUIDToAssetPath(guid);
						BaseFieldBlueprint blueprint = AssetDatabase.LoadAssetAtPath<BaseFieldBlueprint>(assetPath);
						AppendLine($"public static readonly FieldAssetAddress {blueprint.name} = new FieldAssetAddress({blueprint.Id}, \"{blueprint.RuntimeAssetAddress}\", \"{blueprint.name}\");");
					}
				}
				AppendLine();
				using (Class("FieldSettingGenerator", isPartial: true)) {
					using (Function("partial void GenerateInternal(EntityManager entityManager, Entity parent)")) {
						AppendLine($"Entity settingEntity = entityManager.Create(");
						using (Indent) {
							AppendLine($"new FieldSetting {{");
							using (Indent) {
								AppendLine($"loadFieldDistance = {mainSettings.LoadFieldDistance}f,");
								AppendLine($"unloadFieldDistance = {mainSettings.UnloadFieldDistance}f,");
								AppendLine($"cacheFieldMeshCount = {mainSettings.MeshCacheCount},");
								AppendLine($"updateInterval = {mainSettings.UpdateInterval},");
								AppendLine($"belongsTo = Layer.Terrain,");
								AppendLine($"collidesWith = LayerCollide.Terrain,");
							}
							AppendLine($"}},");
							AppendLine($"new Parent {{ Value = parent, }},");
							AppendLine($"LocalTransform.Identity,");
							AppendLine($"new LocalToWorld {{ Value = float4x4.identity, }}");
						}
						AppendLine($");");
						AppendLine($"ECS.SetEntityName(entityManager, settingEntity, \"Field Setting@Hexegeer\");");
					}
				}
			}
		}
	}
}