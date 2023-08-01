using UnityEditor;
using UnityEngine;

namespace Waddle.Authoring.Unity.Extensions
{
    public static class FieldExtensions
    {
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
                Value = (IFieldValue)fieldValue
            };
        }
    }
}