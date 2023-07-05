using System.IO;
using System.Text;
using UnityEditor;
using Waddle.Authoring.Registry;

namespace Waddle.Authoring.PostProcessors
{
    public class ModulePostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType != typeof(ModuleDefinition)) continue;
                var moduleGuid = AssetDatabase.AssetPathToGUID(assetPath);
                foreach (var entityGuid in ModuleRegistry.GetEntitiesWithModule(moduleGuid))
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(entityGuid));
                }

                var moduleDefinition = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(assetPath);
                GenerateAndSaveScript(moduleDefinition, moduleGuid, "Assets/Waddle.Authoring.GeneratedModules");
            }
        }
        
        private static void GenerateAndSaveScript(ModuleDefinition moduleInstance, string scriptName, string folderPath)
        {
            var scriptCode = GenerateClass(moduleInstance);

            // Create directory if not exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Full path for the script
            string fullFilePath = Path.Combine(folderPath, $"{scriptName}.cs");

            // Write script code to the file
            File.WriteAllText(fullFilePath, scriptCode);
            AssetDatabase.Refresh();
        }

        private static string GenerateClass(ModuleDefinition moduleDefinition)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// ReSharper disable BuiltInTypeReferenceStyle");
            sb.AppendLine("namespace Waddle.Authoring.GeneratedModules");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic class {moduleDefinition.name}");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tprivate readonly Entity.ModuleInstance _moduleInstance;");
            sb.AppendLine();
            sb.AppendLine($"\t\tpublic {moduleDefinition.name}(Entity.ModuleInstance moduleInstance)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t_moduleInstance = moduleInstance;");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            
            foreach (var field in moduleDefinition.FieldDefinitions)
            {
                // Get the first generic argument of the FieldType
                var underlyingType = field.FieldType.BaseType;
                underlyingType = underlyingType!.GetGenericArguments()[0];

                string propertyType = underlyingType.FullName;

                // Create the property
                sb.AppendLine(
                    $"\t\tpublic {propertyType} {field.name} => _moduleInstance.GetValue<{propertyType}>(\"{field.name}\");");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
