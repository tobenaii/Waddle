using UnityEditor;
using UnityEngine;

namespace Waddle.Authoring.Core
{
    [System.Serializable]
    public class Field
    {
        public string Name;
        public string TypeID;
        public string FieldID;
        [SerializeReference] public FieldValue Value;
        
        public static Field FromFieldDefinition(FieldDefinition fieldDefinition)
        {
            var fieldValue = ScriptableObject.CreateInstance(AssetDatabase
                .LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(fieldDefinition.TypeID)).GetClass());
            fieldValue.name = fieldDefinition.Name;
            return new Field
            {
                Name = fieldDefinition.Name,
                FieldID = fieldDefinition.FieldID,
                TypeID = fieldDefinition.TypeID,
                Value = (FieldValue)fieldValue
            };
        }
    }
}