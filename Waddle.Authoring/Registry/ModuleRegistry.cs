using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Waddle.Authoring.Registry
{
    [InitializeOnLoad]
    public static class ModuleRegistry
    {
        [System.Serializable]
        private class RegistryState
        {
            public readonly Dictionary<string, List<string>> ModuleEntityMap = new();
            public readonly Dictionary<string, List<string>> EntityModuleMap = new();
        }

        private static readonly RegistryState RegistryMaps = new();

        private static string LibraryPath => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Library/com.waddle.authoring/");
        private static string RegistryPath => Path.Combine(LibraryPath, "ModuleRegistry.json");

        static ModuleRegistry()
        {
            AssemblyReloadEvents.afterAssemblyReload += LoadRegistry;
            LoadRegistry();
        }

        static void LoadRegistry()
        {
            if (!File.Exists(RegistryPath))
            {
                InitialiseRegistry();
                return;
            }
            var json = File.ReadAllText(RegistryPath);
            JsonConvert.PopulateObject(json, RegistryMaps);
        }

        public static IEnumerable<string> GetEntitiesWithModule(string moduleGuid)
        {
            return RegistryMaps.ModuleEntityMap.TryGetValue(moduleGuid, out var entities) ? entities : Enumerable.Empty<string>();
        }

        public static void UpdateModulesForEntity(Entity entity)
        {
            var entityGuid = AssetPathToGUID(entity);
            if (!RegistryMaps.EntityModuleMap.TryGetValue(entityGuid, out var oldModuleGuids))
            {
                oldModuleGuids = new List<string>();
                RegistryMaps.EntityModuleMap.Add(entityGuid, oldModuleGuids);
            }
            var currentModuleGuids = ModuleGuidsFromEntity(entity);

            foreach (var moduleGuid in oldModuleGuids.Except(currentModuleGuids).ToList())
            {
                RemoveEntry(moduleGuid, entityGuid, RegistryMaps.ModuleEntityMap);
                oldModuleGuids.Remove(moduleGuid);
            }

            foreach (var moduleGuid in currentModuleGuids.Except(oldModuleGuids))
            {
                oldModuleGuids.Add(moduleGuid);
                AddEntry(moduleGuid, entityGuid, RegistryMaps.ModuleEntityMap);
            }

            SaveRegistry();
        }

        private static string AssetPathToGUID(Object obj)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        }

        private static List<string> ModuleGuidsFromEntity(Entity entity)
        {
            return entity.ModuleInstances.Select(module => AssetPathToGUID(module.ModuleDefinition)).ToList();
        }

        private static void AddEntry(string key, string value, Dictionary<string, List<string>> map)
        {
            if (!map.TryGetValue(key, out var list))
            {
                list = new List<string>();
                map.Add(key, list);
            }
            list.Add(value);
        }

        private static void RemoveEntry(string key, string value, Dictionary<string, List<string>> map)
        {
            if (!map.TryGetValue(key, out var list)) return;
            list.Remove(value);
            if (list.Count == 0)
            {
                map.Remove(key);
            }
        }

        private static void InitialiseRegistry()
        {
            if (!Directory.Exists(LibraryPath))
            {
                Directory.CreateDirectory(LibraryPath);
            }

            File.WriteAllText(RegistryPath, string.Empty);
            var entityType = typeof(Entity);
            var entityGuids = AssetDatabase.FindAssets($"t:{entityType.Namespace}.{entityType.Name}");
            foreach (var entityGuid in entityGuids)
            {
                var entity = AssetDatabase.LoadAssetAtPath<Entity>(AssetDatabase.GUIDToAssetPath(entityGuid));
                UpdateModulesForEntity(entity);
            }
            SaveRegistry();
        }

        private static void SaveRegistry()
        {
            var json = JsonConvert.SerializeObject(RegistryMaps, Formatting.Indented);
            File.WriteAllText(RegistryPath, json);
        }
    }
}