using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Waddle.Authoring.Core;
using Waddle.SearchWindow;

namespace Waddle.Authoring.Inspectors
{
    [CustomEditor(typeof(ModuleDefinitionContainer))]
    public class ModuleInspector : Editor
    {
        [SerializeField] private VisualTreeAsset _moduleAsset;
        [SerializeField] private VisualTreeAsset _fieldDefinitionAsset;

        private SerializedProperty _fieldDefinitions;
        
        public override VisualElement CreateInspectorGUI()
        {
            _fieldDefinitions = serializedObject.FindProperty("_moduleDefinition.FieldDefinitions");
            
            var root = new VisualElement();
            _moduleAsset.CloneTree(root);
            var listView = root.Q<ListView>();
            listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OpenFieldTypeSearchWindow);
            listView.makeItem += _fieldDefinitionAsset.CloneTree;
            listView.bindItem += (element, i) =>
            {
                serializedObject.Update();
                var fieldDefinition = (FieldDefinition)_fieldDefinitions.GetArrayElementAtIndex(i).boxedValue;
                var dropdown = element.Q<DropdownField>();
                dropdown.choices = new List<string>()
                {
                    ObjectNames.NicifyVariableName(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(fieldDefinition.TypeID)))
                };
                dropdown.index = 0;
                
                element.Unbind();
                element.Q<TextField>().bindingPath = _fieldDefinitions.GetArrayElementAtIndex(i).FindPropertyRelative("Name").propertyPath;
                element.Bind(serializedObject);
            };
            return root;
        }

        private void OpenFieldTypeSearchWindow()
        {
            var searchWindow = CreateInstance<SearchWindow.SearchWindow>();
            
            searchWindow.Items = TypeCache.GetTypesDerivedFrom<FieldValue>()
                .Where(type => !type.IsAbstract)
                .Select(type =>
                new SearchView.Item()
                {
                    Content = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(CreateInstance(type)))),
                    Path = ObjectNames.NicifyVariableName(type.Name),
                })
                .ToList();
            searchWindow.OnSelection += item =>
            {
                var fieldDefinition = new FieldDefinition
                {
                    Name = "New Field",
                    FieldID = GUID.Generate().ToString(),
                    TypeID = (string)item.Content,
                };

                _fieldDefinitions.InsertArrayElementAtIndex(_fieldDefinitions.arraySize);
                _fieldDefinitions.GetArrayElementAtIndex(_fieldDefinitions.arraySize - 1).boxedValue = fieldDefinition;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                Repaint();
            };
            searchWindow.ShowAsDropDown(default, new Vector2(200, 400));
            searchWindow.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(200, 400));
        }
    }
}
