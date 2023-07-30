using Newtonsoft.Json;
using UnityEngine;

namespace Waddle.Authoring.Unity.Fields
{
    public class FloatField : ScriptableObject, IFieldValue
    {
        [SerializeField] private float _value;

        public float Value => _value;
        
        public string Serialize()
        {
            return JsonConvert.SerializeObject(_value);
        }

        public void Deserialize(string json)
        {
            _value = JsonConvert.DeserializeObject<float>(json);
        }
    }
}