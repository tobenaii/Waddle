using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Waddle.Authoring.Registry;

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
            if (entity.ModuleInstances == null)
                return;

            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Field>().ToArray();

            foreach (var moduleInstance in entity.ModuleInstances)
            {
                AddMissingFields(assetPath, entity, moduleInstance, allAssetsAtPath);
                RemoveExtraFields(moduleInstance);
                SortFields(moduleInstance);
                UpdateFieldNames(moduleInstance);
            }

            ModuleRegistry.UpdateModulesForEntity(entity);
        }

        private static void AddMissingFields(string assetPath, Entity entity, Entity.ModuleInstance moduleInstance,
            Field[] allAssetsAtPath)
        {
            var module = moduleInstance.ModuleDefinition;
            foreach (var fieldDefinition in module.FieldDefinitions.Where(fieldDef => moduleInstance.Fields.All(field => field.FieldDefinition != fieldDef)))
            {
                var entityField = GetExistingField(allAssetsAtPath, fieldDefinition) ??
                                  CreateField(entity, fieldDefinition);
                entityField.FieldDefinition = fieldDefinition;
                moduleInstance.Fields.Add(entityField);
            }
        }

        private static Field GetExistingField(IEnumerable<Field> allAssetsAtPath, Object fieldDefinition)
        {
            return allAssetsAtPath.FirstOrDefault(existingField => existingField.FieldDefinition == fieldDefinition);
        }

        private static Field CreateField(Object entity, FieldDefinition fieldDefinition)
        {
            var entityField = (Field)ScriptableObject.CreateInstance(fieldDefinition.FieldType);
            AssetDatabase.AddObjectToAsset(entityField, entity);
            AssetDatabase.Refresh();
            return entityField;
        }

        private static void RemoveExtraFields(Entity.ModuleInstance moduleInstance)
        {
            var module = moduleInstance.ModuleDefinition;
            foreach (var entityField in moduleInstance.Fields
                         .Where(entityField => !module.FieldDefinitions.Contains(entityField.FieldDefinition)).ToList())
            {
                moduleInstance.Fields.Remove(entityField);
            }
        }

        private static void SortFields(Entity.ModuleInstance moduleInstance)
        {
            var module = moduleInstance.ModuleDefinition;
            moduleInstance.Fields = moduleInstance.Fields
                .OrderBy(field => module.FieldDefinitions.IndexOf(field.FieldDefinition)).ToList();
        }
        
        private static void UpdateFieldNames(Entity.ModuleInstance moduleInstance)
        {
            foreach (var field in moduleInstance.Fields)
            {
                if (field.name == field.FieldDefinition.name) continue;
                
                field.name = field.FieldDefinition.name;
                EditorUtility.SetDirty(field);
            }
        }
    }
}
