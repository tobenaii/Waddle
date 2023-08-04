using UnityEngine;

namespace Waddle.Authoring.Core
{
    public abstract class FieldValue : ScriptableObject
    {
        public abstract string Serialize();
        public abstract void Deserialize(string json);
    }
}