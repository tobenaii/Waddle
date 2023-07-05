using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Waddle.Authoring
{
    [CreateAssetMenu(menuName = "Waddle/Module")]
    public class ModuleDefinition : ScriptableObject
    {
        [SerializeField] private List<FieldDefinition> _fieldDefinitions;

        public List<FieldDefinition> FieldDefinitions => _fieldDefinitions;
    }
}