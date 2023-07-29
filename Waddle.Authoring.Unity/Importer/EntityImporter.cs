using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Waddle.Authoring.Unity.Importer
{
    [ScriptedImporter(1, "entity")]
    public class EntityImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var entityContainer = ScriptableObject.CreateInstance<EntityContainer>()
                .FromJson(File.ReadAllText(ctx.assetPath));

            foreach (var module in entityContainer.Modules)
            {
                ctx.DependsOnSourceAsset(new GUID(module.ModuleID));
            }
            
            var mainObj = new GameObject("entity obj");
            ctx.AddObjectToAsset("entity obj", mainObj);
            ctx.SetMainObject(mainObj);
        }
    }
}
