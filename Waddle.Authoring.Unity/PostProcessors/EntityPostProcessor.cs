using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Waddle.Authoring.Unity.Extensions;
using Waddle.Authoring.Unity.Importer;

namespace Waddle.Authoring.Unity.PostProcessors
{
public class EntityPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                if (Path.GetExtension(assetPath) != ".entity") continue;
                Process(assetPath);
            }
        }

        private static void Process(string assetPath)
        {
            var json = File.ReadAllText(assetPath);
            
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new FieldConverter());
            
            var entity = JsonConvert.DeserializeObject<Entity>(json, settings) ?? new Entity()
            {
                Modules = new List<Module>()
            };

            foreach (var module in entity.Modules)
            {
                var moduleDefinition = AssetDatabase.LoadAssetAtPath<ModuleDefinitionContainer>(
                    AssetDatabase.GUIDToAssetPath(module.ModuleID));
                if (moduleDefinition == null)
                {
                    continue;
                }
                module.Name = moduleDefinition.name;
        
                var fieldIDsToRemove = new List<string>();

                foreach (var field in module.Fields)
                {
                    var fieldDefinition = moduleDefinition.FieldDefinitions.FirstOrDefault(
                        fd => fd.FieldID == field.FieldID);
                    if (fieldDefinition != null)
                    {
                        field.Name = fieldDefinition.Name;
                    }
                    else
                    {
                        fieldIDsToRemove.Add(field.FieldID);
                    }
                }

                module.Fields.RemoveAll(field => fieldIDsToRemove.Contains(field.FieldID));

                var newFields = moduleDefinition.FieldDefinitions
                    .Where(fieldDefinition => module.Fields.All(field => field.FieldID != fieldDefinition.FieldID))
                    .Select(FieldExtensions.FromFieldDefinition);

                module.Fields.AddRange(newFields);
                    
                module.Fields = module.Fields
                    .OrderBy(field => moduleDefinition.FieldDefinitions.ToList().FindIndex(fd => fd.FieldID == field.FieldID))
                    .ToList();
            }

            var newJson = JsonConvert.SerializeObject(entity, settings);

            if (newJson != json)
            {
                File.WriteAllText(assetPath, newJson);
            }
        }
    }
}