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
                var fieldInstance = _fieldDefinitions.GetArrayElementAtIndex(i);
                var dropdown = element.Q<DropdownField>();
                dropdown.choices = new List<string>()
                {
                    fieldInstance.FindPropertyRelative("FieldType")
                        .objectReferenceValue.name
                };
                dropdown.index = 0;

                element.Q<TextField>().BindProperty(fieldInstance.FindPropertyRelative("Name"));
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
                _fieldDefinitions.InsertArrayElementAtIndex(_fieldDefinitions.arraySize);
                _fieldDefinitions.GetArrayElementAtIndex(_fieldDefinitions.arraySize - 1).boxedValue = new ModuleDefinition.FieldDefinition()
                {
                    Name = "New Field",
                    ID = Guid.NewGuid().ToString(),
                    FieldType = MonoScript.FromScriptableObject(CreateInstance((Type)item.Content)),
                };
                _fieldDefinitions.serializedObject.ApplyModifiedProperties();
                
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                Repaint();
            };
            searchWindow.ShowAsDropDown(default, new Vector2(200, 400));
            searchWindow.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(200, 400));
        }
    }
}
