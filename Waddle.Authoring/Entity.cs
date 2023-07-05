using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

            public T GetValue<T>(string name)
            {
                return ((Field<T>)Fields.First(field => field.name == name)).Value;
            }
        }

        [SerializeField] private List<ModuleInstance> _modules;

        public List<ModuleInstance> ModuleInstances => _modules;
    }
}