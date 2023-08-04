using System.IO;
using System.Text;
using UnityEditor;

namespace Waddle.Authoring.PostProcessors
{
    public class ModulePostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                if (Path.GetExtension(assetPath) != ".module") continue;
                Process(assetPath);
            }
        }

        private static void Process(string assetPath)
        {
            var moduleDefinition = AssetDatabase.LoadAssetAtPath<ModuleDefinitionContainer>(assetPath);
            GenerateAndSaveScript(moduleDefinition, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(moduleDefinition)), "Assets/Waddle.Authoring.GeneratedModules");
        }

        private static void GenerateAndSaveScript(ModuleDefinitionContainer moduleDefinition, string scriptName,
            string folderPath)
        {
            var scriptCode = GenerateClass(moduleDefinition);

            // Create directory if not exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Full path for the script
            string fullFilePath = Path.Combine(folderPath, $"{scriptName}.cs");

            // Write script code to the file
            File.WriteAllText(fullFilePath, scriptCode);
        }

        private static string GenerateClass(ModuleDefinitionContainer moduleDefinition)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("namespace Waddle.Authoring.GeneratedModules");
            sb.AppendLine("{");
            sb.AppendLine(
                $"\tpublic class {moduleDefinition.name} : UnityEngine.MonoBehaviour, Waddle.Authoring.Baking.IModuleWrapper");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\t[UnityEngine.SerializeField] private Waddle.Authoring.Core.Module _module;");
            sb.AppendLine();
            int index = 0;
            foreach (var field in moduleDefinition.FieldDefinitions)
            {
                var fieldType = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(field.TypeID))
                    .GetClass().FullName;

                // Create the property
                sb.AppendLine(
                    $"\t\tpublic {fieldType} {field.Name} => ({fieldType})_module.Fields[{index}].Value;");
                index++;
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}