using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Waddle.Authoring.Inspectors
{
    [CustomEditor(typeof(Module))]
    public class ModuleInspector : Editor
    {
        [SerializeField] private VisualTreeAsset _moduleAsset;
        [SerializeField] private VisualTreeAsset _fieldDefinitionAsset;
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            _moduleAsset.CloneTree(root);
            var listView = root.Q<ListView>();
            listView.itemsAdded += FieldsAdded;
            listView.makeItem += _fieldDefinitionAsset.CloneTree;
            return root;
        }

        private void FieldsAdded(IEnumerable<int> indices)
        {
            var fieldsList = serializedObject.FindProperty("_fieldDefinitions");
            foreach (var index in indices)
            {
                var fieldProperty = fieldsList.GetArrayElementAtIndex(index);
                fieldProperty.FindPropertyRelative("Name").stringValue = "New Field";
                fieldProperty.FindPropertyRelative("ID").stringValue = Guid.NewGuid().ToString();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
