using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Waddle.Authoring.Unity.Baking;

namespace Waddle.Authoring.Unity.Importer
{
    [ScriptedImporter(1, "entity")]
    public class EntityImporter : ScriptedImporter
    {
        private EntityContainer _entityContainer;
        private static readonly Dictionary<Type, ModuleBaker> ModuleBakerMap = new();

        [InitializeOnLoadMethod]
        private static void InitializeBakerMap()
        {
            foreach (var moduleType in TypeCache.GetTypesDerivedFrom<IModuleWrapper>()
                         .Where(type => !type.IsAbstract && !type.IsInterface))
            {
                var bakerType = TypeCache
                    .GetTypesDerivedFrom<ModuleBaker>()
                    .FirstOrDefault(type => !type.IsAbstract && type.BaseType!.IsGenericType &&
                                            type.BaseType.GenericTypeArguments[0] == moduleType);

                if (bakerType != null)
                {
                    ModuleBakerMap.Add(moduleType, (ModuleBaker)Activator.CreateInstance(bakerType));
                }
            }
        }
        
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var json = File.ReadAllText(ctx.assetPath);

            var entityContainer = ScriptableObject.CreateInstance<EntityContainer>().FromJson(json);
            
            var mainObj = new GameObject("entity obj");

            foreach (var module in entityContainer.Modules)
            {
                foreach (var field in module.Fields)
                {
                    field.Value.name =
                        $"{Path.GetFileNameWithoutExtension(ctx.assetPath)}.{module.Name}.{field.Value.name}";
                    ctx.AddObjectToAsset($"{field.FieldID} field obj", field.Value);
                }
                
                ctx.DependsOnSourceAsset(new GUID(module.ModuleID));
                var monoScript =
                    AssetDatabase.LoadAssetAtPath<MonoScript>(
                        $"Assets/Waddle.Authoring.GeneratedModules/{module.ModuleID}.cs");
                if (monoScript != null)
                {
                    var moduleType = monoScript.GetClass();
                    var moduleWrapper = mainObj.AddComponent(moduleType);

                    var serializedModule = new SerializedObject(moduleWrapper);
                    serializedModule.FindProperty("_module").boxedValue = module;
                    serializedModule.ApplyModifiedProperties();
                    
                    if (ModuleBakerMap.TryGetValue(moduleType, out var baker))
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        baker.Bake(mainObj, (IModuleWrapper)moduleWrapper);
                    }
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
