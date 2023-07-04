using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Waddle.Authoring
{
    [CreateAssetMenu(menuName = "Waddle/Entity", order = -1)]
    public class Entity : ScriptableObject
    {
        [System.Serializable]
        public class ModuleInstance
        {
            public ModuleDefinition ModuleDefinition;
            public List<Field> Fields;
        }

        [SerializeField] private List<ModuleInstance> _modules;

        public List<ModuleInstance> Modules => _modules;
    }
}