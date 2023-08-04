using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Waddle.Authoring.Core;

namespace Waddle.Authoring
{
    public class ModuleDefinitionContainer : ScriptableObject
    {
        [SerializeField] private ModuleDefinition _moduleDefinition;

        public IList<FieldDefinition> FieldDefinitions => _moduleDefinition.FieldDefinitions;
        
        public string ToJson()
        {
            return JsonConvert.SerializeObject(_moduleDefinition);
        }

        public void FromJson(string name, string guid, string json)
        {
            var moduleDefinition = JsonConvert.DeserializeObject<ModuleDefinition>(json) ?? new ModuleDefinition()
            {
                FieldDefinitions = new List<FieldDefinition>()
            };
            moduleDefinition.Name = name;
            moduleDefinition.ModuleID = guid;
            this.name = name;
            _moduleDefinition = moduleDefinition;
        }
        
        public Module ToModule()
        {
            var module = new Module
            {
                Name = _moduleDefinition.Name,
                ModuleID = _moduleDefinition.ModuleID,
                Fields = new List<Field>(_moduleDefinition.FieldDefinitions.Count),
            };

            foreach (var fieldDefinition in _moduleDefinition.FieldDefinitions)
            {
                var field = Field.FromFieldDefinition(fieldDefinition);
                module.Fields.Add(field);
            }
            return module;
        }
    }
}