using UnityEngine;

namespace Waddle.Authoring
{
    public abstract class Field : ScriptableObject
    {
        public string ID;
    }
    
    public abstract class Field<T> : Field
    {
        public abstract T Value { get; }
    }
}