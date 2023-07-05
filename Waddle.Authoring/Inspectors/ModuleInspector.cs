using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Waddle.SearchWindow;
using Button = UnityEngine.UIElements.Button;

namespace Waddle.Authoring.Inspectors
{
    [CustomEditor(typeof(ModuleDefinition))]
    public class ModuleInspector : Editor
    {
        [SerializeField] private VisualTreeAsset _moduleAsset;
        [SerializeField] private VisualTreeAsset _fieldDefinitionAsset;

        private SerializedProperty _fieldDefinitions;
        
        public override VisualElement CreateInspectorGUI()
        {
            _fieldDefinitions = serializedObject.FindProperty("_fieldDefinitions");
            
            var root = new VisualElement();
            _moduleAsset.CloneTree(root);
            var listView = root.Q<ListView>();
            listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OpenFieldTypeSearchWindow);
            listView.makeItem += _fieldDefinitionAsset.CloneTree;
            listView.bindItem += (element, i) =>
            {
                var fieldDefinition = (FieldDefinition)_fieldDefinitions.GetArrayElementAtIndex(i).objectReferenceValue;
                var dropdown = element.Q<DropdownField>();
                dropdown.choices = new List<string>()
                {
                    ObjectNames.NicifyVariableName(fieldDefinition.FieldType.Name)
                };
                dropdown.index = 0;

                element.Q<TextField>().Bind(new SerializedObject(fieldDefinition));
            };
            return root;
        }

        private void OpenFieldTypeSearchWindow()
        {
            var searchWindow = CreateInstance<Waddle.SearchWindow.SearchWindow>();

            searchWindow.Items = TypeCache.GetTypesDerivedFrom<Field>()
                .Where(type => !type.IsAbstract)
                .Select(type =>
                new SearchView.Item()
                {
                    Content = type,
                    Path = ObjectNames.NicifyVariableName(type.Name),
                })
                .ToList();
            searchWindow.OnSelection += item =>
            {
                var fieldDefinition = CreateInstance<FieldDefinition>();
                fieldDefinition.FieldType = (Type)item.Content;
                fieldDefinition.name = "New Field";
                
                AssetDatabase.AddObjectToAsset(fieldDefinition, target);
                AssetDatabase.Refresh();
                
                _fieldDefinitions.InsertArrayElementAtIndex(_fieldDefinitions.arraySize);
                _fieldDefinitions.GetArrayElementAtIndex(_fieldDefinitions.arraySize - 1).objectReferenceValue = fieldDefinition;
                _fieldDefinitions.serializedObject.ApplyModifiedProperties();
                
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                Repaint();
            };
            searchWindow.ShowAsDropDown(default, new Vector2(200, 400));
            searchWindow.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(200, 400));
        }
    }
}
