using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Waddle.Authoring
{
    [CreateAssetMenu(menuName = "Waddle/Module")]
    public class ModuleDefinition : ScriptableObject
    {
        [Serializable]
        public class FieldDefinition
        {
            public string Name;
            public MonoScript FieldType;
            public string ID;
        }
        
        [SerializeField] private List<FieldDefinition> _fieldDefinitions;

        public List<FieldDefinition> FieldDefinitions => _fieldDefinitions;
    }
}