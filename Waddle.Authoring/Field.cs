using UnityEngine;

namespace Waddle.Authoring
{
    public abstract class Field : ScriptableObject
    {
        [HideInInspector, SerializeField] private FieldDefinition _fieldDefinition;

        public FieldDefinition FieldDefinition
        {
            get;
            internal set;
        }
    }
    
    public abstract class Field<T> : Field
    {
        public abstract T Value { get; }
    }
}