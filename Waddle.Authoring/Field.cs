using UnityEngine;

namespace Waddle.Authoring
{
    public abstract class Field : ScriptableObject
    {
        [HideInInspector] public string ID;
    }
    
    public abstract class Field<T> : Field
    {
        public abstract T Value { get; }
    }
}