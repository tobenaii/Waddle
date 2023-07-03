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
        [System.Serializable]
        private class RegistryMaps
        {
            public readonly Dictionary<string, List<string>> ModuleEntityMap = new();
            public readonly Dictionary<string, List<string>> EntityModuleMap = new();
        }

        private static RegistryMaps _registryMaps;
        
        private static string LibraryPath =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Library/com.waddle.authoring/");

        private static string RegistryPath => Path.Combine(LibraryPath, "ModuleRegistry.json");

        private static FileSystemWatcher _watcher;

        static ModuleRegistry()
        {
            AssemblyReloadEvents.afterAssemblyReload += LoadRegistry;
        }
        
        static void LoadRegistry()
        {
            if (!File.Exists(RegistryPath))
            {
                InitialiseRegistry();
                return;
            }
            var json = File.ReadAllText(RegistryPath);
            _registryMaps = JsonConvert.DeserializeObject<RegistryMaps>(json);
        }
        
        static void InitialiseRegistry()
        {
            if (!Directory.Exists(LibraryPath))
            {
                Directory.CreateDirectory(LibraryPath);
            }
            
            File.Create(RegistryPath).Dispose();
            _registryMaps = new RegistryMaps();
            var entityType = typeof(Entity);
            var entityGuids = AssetDatabase.FindAssets($"t:{entityType.Namespace}.{entityType.Name}");
            foreach (var entityGuid in entityGuids)
            {
                var entity = AssetDatabase.LoadAssetAtPath<Entity>(AssetDatabase.GUIDToAssetPath(entityGuid));
                if (!_registryMaps.EntityModuleMap.TryGetValue(entityGuid, out var moduleGuids))
                {
                    moduleGuids = new List<string>();
                    _registryMaps.EntityModuleMap.Add(entityGuid, moduleGuids);
                }
                foreach (var moduleGuid in entity.Modules.Select(module => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(module.Module))))
                {
                    moduleGuids.Add(moduleGuid);
                    if (!_registryMaps.ModuleEntityMap.TryGetValue(moduleGuid, out var mappedEntityGuids))
                    {
                        mappedEntityGuids = new List<string>();
                        _registryMaps.ModuleEntityMap.Add(moduleGuid, mappedEntityGuids);
                    }
                    mappedEntityGuids.Add(entityGuid);
                }
            }
            SaveRegistry();
        }

        private static void SaveRegistry()
        {
            if (_registryMaps.ModuleEntityMap == null) return;
            var json = JsonConvert.SerializeObject(_registryMaps, Formatting.Indented);
            File.WriteAllText(RegistryPath, json);
        }

        public static IEnumerable<string> GetEntitiesWithModule(string moduleGuid)
        {
            return _registryMaps.ModuleEntityMap.TryGetValue(moduleGuid, out var entities) ? entities : Enumerable.Empty<string>();
        }

        public static void UpdateEntityWithModule(Entity entity)
        {
            var entityGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entity));
            
            if (!_registryMaps.EntityModuleMap.TryGetValue(entityGuid, out var moduleGuids))
            {
                moduleGuids = new List<string>();
            }
            
            foreach (var prevModule in moduleGuids)
            {
                _registryMaps.ModuleEntityMap[prevModule].Remove(entityGuid);
            }
            
            moduleGuids.Clear();
            
            foreach (var moduleGuid in entity.Modules.Select(module => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(module.Module))))
            {
                moduleGuids.Add(moduleGuid);
                if (!_registryMaps.ModuleEntityMap.TryGetValue(moduleGuid, out var entityGuids))
                {
                    entityGuids = new List<string>();
                    _registryMaps.ModuleEntityMap.Add(moduleGuid, entityGuids);
                }
                entityGuids.Add(entityGuid);
            }
            SaveRegistry();
        }
    }
}
