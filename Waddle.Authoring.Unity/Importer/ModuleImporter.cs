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
        }
    }
}