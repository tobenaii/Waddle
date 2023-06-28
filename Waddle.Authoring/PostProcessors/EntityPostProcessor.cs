using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Waddle.Authoring.PostProcessors
{
    public class EntityPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (assetType != typeof(Entity)) continue;
                Process(assetPath);
            }
        }

        private static void Process(string assetPath)
        {
            var entity = AssetDatabase.LoadAssetAtPath<Entity>(assetPath);
            if (entity.Modules == null) return;
            foreach (var moduleInstance in entity.Modules)
            {
                var module = moduleInstance.Module;

                foreach (var moduleField in module.FieldDefinitions)
                {
                    if (moduleInstance.Fields.FirstOrDefault(field => field.ID == moduleField.ID) != null) continue;

                    Field entityField = null;
                    foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
                    {
                        if (obj is not Field existingField) continue;
                        if (existingField.ID != moduleField.ID) continue;
                        entityField = existingField;
                    }

                    if (entityField == null)
                    {
                        entityField = (Field)ScriptableObject.CreateInstance(moduleField.FieldType.GetClass());
                        AssetDatabase.AddObjectToAsset(entityField, entity);
                        AssetDatabase.Refresh();
                    }
                    entityField.name = moduleField.Name;
                    entityField.ID = moduleField.ID;
                    moduleInstance.Fields.Add(entityField);
                }

                foreach (var entityField in moduleInstance.Fields.ToList())
                {
                    if (module.FieldDefinitions.FirstOrDefault(field => field.ID == entityField.ID) != null) continue;
                    moduleInstance.Fields.Remove(entityField);
                }
            }
        }
    }
}
