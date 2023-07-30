using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Waddle.Authoring.Unity.Extensions;

namespace Waddle.Authoring.Unity.Importer
{
    [ScriptedImporter(1, "entity")]
    public class EntityImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var json = File.ReadAllText(ctx.assetPath);
            
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new FieldConverter());
            
            var entity = JsonConvert.DeserializeObject<Entity>(json, settings) ?? new Entity()
            {
                Modules = new List<Module>()
            };

            var mainObj = new GameObject("entity obj");

            foreach (var module in entity.Modules)
            {
                ctx.DependsOnSourceAsset(new GUID(module.ModuleID));
                var monoScript =
                    AssetDatabase.LoadAssetAtPath<MonoScript>(
                        $"Assets/Waddle.Authoring.GeneratedModules/{module.ModuleID}.cs");
                if (monoScript != null)
                {
                    mainObj.AddComponent(monoScript.GetClass());
                }
            }

            ctx.AddObjectToAsset("entity obj", mainObj);
            ctx.SetMainObject(mainObj);
        }
    }
    
    internal class FieldConverter : JsonConverter<Field>
    {
        public override void WriteJson(JsonWriter writer, Field field, JsonSerializer serializer)
        {
            var jo = new JObject
            {
                { "Name", field.Name },
                { "FieldID", field.FieldID},
                { "TypeID", field.TypeID },
                { "Value", field.Value.Serialize() }
            };
            jo.WriteTo(writer);
        }

        public override Field ReadJson(JsonReader reader, Type objectType, Field existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var name = jo["Name"]!.Value<string>();
            var fieldID = jo["FieldID"]!.Value<string>();
            var typeID = jo["TypeID"]!.Value<string>();
            var value = jo["Value"]!.Value<string>();

            var valueType = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(typeID)).GetClass();
            var valueInstance = ScriptableObject.CreateInstance(valueType) as IFieldValue;
            valueInstance!.Deserialize(value);
            
            var newField = new Field
            {
                Name = name,
                FieldID = fieldID,
                TypeID = typeID,
                Value = valueInstance
            };

            return newField;
        }
    }
}
