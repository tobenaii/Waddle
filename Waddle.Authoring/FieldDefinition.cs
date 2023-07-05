using System;
using UnityEditor;
using UnityEngine;

namespace Waddle.Authoring
{
    public class FieldDefinition : ScriptableObject
    {
        [SerializeField] private MonoScript _fieldScript;

        public Type FieldType
        {
            get => _fieldScript.GetClass();
            set => _fieldScript = MonoScript.FromScriptableObject(CreateInstance(value));
        }
    }
}