using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Waddle.Authoring.Registry
{
    [InitializeOnLoad]
    public static class ModuleRegistry
    {
        private static Dictionary<string, List<string>> _moduleEntityMap;
        
        private static string LibraryPath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Library/com.waddle.authoring/");

        private static string RegistryPath => Path.Combine(LibraryPath, "ModuleRegistry.json");

        private static FileSystemWatcher _watcher;

        static ModuleRegistry()
        {
            AssemblyReloadEvents.afterAssemblyReload += LoadRegistry;
        }
        
        static void InitialiseRegistry()
        {
            if (!Directory.Exists(LibraryPath))
            {
                Directory.CreateDirectory(LibraryPath);
            }
            
            File.Create(RegistryPath).Dispose();
            _moduleEntityMap = new Dictionary<string, List<string>>();
            var entityType = typeof(Entity);
            var entityGuids = AssetDatabase.FindAssets($"t:{entityType.Namespace}.{entityType.Name}");
            foreach (var entityGuid in entityGuids)
            {
                var entity = AssetDatabase.LoadAssetAtPath<Entity>(AssetDatabase.GUIDToAssetPath(entityGuid));
                foreach (var module in entity.Modules)
                {
                    var moduleGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(module.Module));
                    if (!_moduleEntityMap.TryGetValue(moduleGuid, out var mappedEntityGuids))
                    {
                        mappedEntityGuids = new List<string>();
                        _moduleEntityMap.Add(moduleGuid, mappedEntityGuids);
                    }
                    mappedEntityGuids.Add(entityGuid);
                }
            }
        }

        static void SaveRegistry()
        {
            if (_moduleEntityMap == null) return;
            string json = JsonConvert.SerializeObject(_moduleEntityMap, Formatting.Indented);
            File.WriteAllText(RegistryPath, json);
        }

        static void LoadRegistry()
        {
            if (!File.Exists(RegistryPath))
            {
                InitialiseRegistry();
                return;
            }
            string json = File.ReadAllText(RegistryPath);
            _moduleEntityMap = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        }

        public static IEnumerable<string> GetEntitiesWithModule(string moduleGuid)
        {
            return _moduleEntityMap.TryGetValue(moduleGuid, out var entities) ? entities : Enumerable.Empty<string>();
        }

        public static void RegisterEntityWithModule(Entity entity, Module module)
        {
            var entityGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entity));
            var moduleGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(module));
            if (!_moduleEntityMap.TryGetValue(moduleGuid, out var entityGuids))
            {
                entityGuids = new List<string>();
                _moduleEntityMap.Add(moduleGuid, entityGuids);
            }
            entityGuids.Add(entityGuid);
            SaveRegistry();
        }

        public static void DeregisterEntityFromModule(Entity entity, Module module)
        {
            var entityGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entity));
            var moduleGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(module));
            _moduleEntityMap[moduleGuid].Remove(entityGuid);
            SaveRegistry();
        }
    }
}
