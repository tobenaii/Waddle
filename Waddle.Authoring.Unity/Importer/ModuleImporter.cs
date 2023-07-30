using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Waddle.Authoring.Unity.Importer
{
    [ScriptedImporter(1, "module")]
    public class ModuleImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var moduleDefinition = ScriptableObject.CreateInstance<ModuleDefinitionContainer>();
            moduleDefinition.FromJson(Path.GetFileNameWithoutExtension(ctx.assetPath), AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this)), File.ReadAllText(ctx.assetPath!));
            ctx.AddObjectToAsset("module definition", moduleDefinition);
            ctx.SetMainObject(moduleDefinition);
            
            GenerateAndSaveScript(moduleDefinition, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this)), "Assets/Waddle.Authoring.GeneratedModules");
        }
        
        private static void GenerateAndSaveScript(ModuleDefinitionContainer moduleDefinition, string scriptName, string folderPath)
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

            sb.AppendLine("// ReSharper disable BuiltInTypeReferenceStyle");
            sb.AppendLine("namespace Waddle.Authoring.GeneratedModules");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic class {moduleDefinition.name} : UnityEngine.MonoBehaviour");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tprivate readonly Waddle.Authoring.Module _module;");
            sb.AppendLine();
            sb.AppendLine($"\t\tpublic {moduleDefinition.name}(Waddle.Authoring.Module module)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t_module = module;");
            sb.AppendLine("\t\t}");
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