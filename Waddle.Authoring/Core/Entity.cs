using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Waddle.Authoring.Core
{
    [System.Serializable]
    public class Entity
    {
        public List<Module> Modules;
        
        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new FieldConverter());
            return JsonConvert.SerializeObject(this, settings);
        }

        public static Entity FromJson(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new FieldConverter());
            
            var entity = JsonConvert.DeserializeObject<Entity>(json, settings) ?? new Entity()
            {
                Modules = new List<Module>()
            };
            return entity;
        }
        
        private class FieldConverter : JsonConverter<Field>
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
                
                // ReSharper disable once SuspiciousTypeConversion.Global
                var valueInstance = (FieldValue)ScriptableObject.CreateInstance(valueType);
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
}