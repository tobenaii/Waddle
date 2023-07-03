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
                if (assetType != typeof(Module)) continue;
                var moduleGuid = AssetDatabase.AssetPathToGUID(assetPath);
                foreach (var entityGuid in ModuleRegistry.GetEntitiesWithModule(moduleGuid))
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(entityGuid));
                }
            }
        }
    }
}
