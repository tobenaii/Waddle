using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Waddle.Authoring.Baking;

namespace Waddle.Authoring.Importer
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

            foreach (var module in entityContainer.Entity.Modules)
            {
                foreach (var field in module.Fields)
                {
                    var valueObject = field.Value;
                    valueObject.name =
                        $"{Path.GetFileNameWithoutExtension(ctx.assetPath)}.{module.Name}.{field.Name}";
                    ctx.AddObjectToAsset($"{field.FieldID} field obj", valueObject);
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
}
