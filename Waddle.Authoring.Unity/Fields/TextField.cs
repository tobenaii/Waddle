using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Localization;

namespace Waddle.Authoring.Unity.Fields
{
    public class TextField : ScriptableObject, IFieldValue
    {
        [SerializeField] private LocalizedString _localizedString;
        
        public string Serialize()
        {
            string combinedString = (_localizedString?.TableReference + "|" + _localizedString?.TableEntryReference.KeyId);
            return JsonConvert.SerializeObject(combinedString);
        }

        public void Deserialize(string json)
        {
            string combinedString = JsonConvert.DeserializeObject<string>(json);
            string[] parts = combinedString.Split('|');

            _localizedString = new LocalizedString(parts[0], (long)Convert.ToDouble(parts[1]));
        }
    }
}