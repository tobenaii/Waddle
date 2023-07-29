using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Waddle.Authoring.Unity.Extensions;

namespace Waddle.Authoring.Unity
{
    public class EntityContainer : ScriptableObject
    {
        [Serializable]
        public class ModuleWrapper
        {
            [Serializable]
            public class FieldWrapper
            {
                [SerializeField] private string _name;
                [SerializeField] private string _fieldID;
                [SerializeField] private string _typeID;
                [SerializeField] private ScriptableObject _value;

                public string FieldID => _fieldID;
                public ScriptableObject Value => _value;
                
                public Field ToField()
                {
                    return new Field()
                    {
                        Name = _name,
                        FieldID = _fieldID,
                        TypeID = _typeID,
                        Value = (IFieldValue)_value,
                    };
                }

                public static FieldWrapper FromField(Field field)
                {
                    return new FieldWrapper()
                    {
                        _name = field.Name,
                        _fieldID = field.FieldID,
                        _typeID = field.TypeID,
                        _value = (ScriptableObject)field.Value
                    };
                }
            }
            
            [SerializeField] private string _name;
            [SerializeField] private List<FieldWrapper> _fields;
            [SerializeField] private string _moduleID;

            public IEnumerable<FieldWrapper> Fields => _fields;
            public string ModuleID => _moduleID;

            public Module ToModule()
            {
                return new Module()
                {
                    Name = _name,
                    Fields = _fields.Select(field => field.ToField()).ToList(),
                    ModuleID = _moduleID,
                };
            }

            public static ModuleWrapper FromModule(Module module)
            {
                var fields = module.Fields
                    .Select(FieldWrapper.FromField)
                    .ToList();
                
                return new ModuleWrapper()
                {
                    _name = module.Name,
                    _fields = fields,
                    _moduleID = module.ModuleID
                };
            }
        }
        [SerializeField] private List<ModuleWrapper> _modules;

        public IEnumerable<ModuleWrapper> Modules => _modules;

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new FieldConverter());
            
            var entity = new Entity
            {
                Modules = _modules.Select(x => x.ToModule()).ToList()
            };
            return JsonConvert.SerializeObject(entity, settings);
        }

        public EntityContainer FromJson(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new FieldConverter());
            
            var entity = JsonConvert.DeserializeObject<Entity>(json, settings) ?? new Entity()
            {
                Modules = new List<Module>()
            };

            if (entity.Modules != null)
            {
                foreach (var module in entity.Modules)
                {
                    var moduleDefinition = AssetDatabase.LoadAssetAtPath<ModuleDefinitionContainer>(
                        AssetDatabase.GUIDToAssetPath(module.ModuleID));
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
                }
            }
            
            _modules = entity.Modules?
                .Where(module => AssetDatabase.LoadAssetAtPath<ModuleDefinitionContainer>(AssetDatabase.GUIDToAssetPath(module.ModuleID)))
                .Select(ModuleWrapper.FromModule)
                .ToList() ?? new List<ModuleWrapper>();
            return this;
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
}