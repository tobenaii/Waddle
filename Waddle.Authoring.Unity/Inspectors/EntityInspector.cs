using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Waddle.SearchWindow;

namespace Waddle.Authoring.Unity.Inspectors
{
    [CustomEditor(typeof(EntityContainer))]
    public class EntityInspector : Editor
    {
        [SerializeField] private VisualTreeAsset _editorAsset;
        [SerializeField] private VisualTreeAsset _itemAsset;

        private static readonly Vector2 WindowSize = new Vector2(200, 400);

        public override bool UseDefaultMargins() => false;

        private SerializedProperty _modulesProperty;
        
        public override VisualElement CreateInspectorGUI()
        {
            _modulesProperty = serializedObject.FindProperty("_modules");
            
            var root = _editorAsset.CloneTree();
            root.Q<Button>("AddModuleButton").clicked += OpenModuleSearchWindow;
            var listView = root.Q<ListView>();
            listView.makeItem = _itemAsset.CloneTree;
            listView.bindItem = (element, moduleIndex) => BindItem(element, moduleIndex);

            return root;
        }

        private void BindItem(VisualElement element, int moduleIndex)
        {
            serializedObject.Update();
            var moduleProperty = _modulesProperty.GetArrayElementAtIndex(moduleIndex);
            
            var fieldList = element.Q<Foldout>();
            fieldList.AddManipulator(new ContextualMenuManipulator(menu =>
            {
                menu.menu.AppendAction("Remove Module", _ => RemoveModule(moduleIndex));
            }));

            fieldList.text = ObjectNames.NicifyVariableName(moduleProperty.FindPropertyRelative("_name").stringValue);

            var fieldsRoot = fieldList.Q("FieldsRoot");
            fieldsRoot.Clear();

            var fieldsProperty = moduleProperty.FindPropertyRelative("_fields");
            for (int i = 0; i < fieldsProperty.arraySize; i++)
            {
                var fieldRoot = CreateFieldInspector(fieldsProperty.GetArrayElementAtIndex(i));
                fieldsRoot.Add(fieldRoot);
            }
        }
        
        private VisualElement CreateFieldInspector(SerializedProperty fieldProperty)
        {
            var fieldRoot = new VisualElement();
            var field = new SerializedObject(fieldProperty.FindPropertyRelative("_value").objectReferenceValue);
            InspectorElement.FillDefaultInspector(fieldRoot, field, this);
            fieldRoot.RemoveAt(0);
            if(fieldRoot.childCount == 1)
            {
                fieldRoot[0].Q<PropertyField>().label = fieldProperty.FindPropertyRelative("_name").stringValue;
            }
            fieldRoot.Bind(field);
            return fieldRoot;
        }

        private void RemoveModule(int moduleIndex)
        {
            _modulesProperty.DeleteArrayElementAtIndex(moduleIndex);
            _modulesProperty.serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    
        
        private void OpenModuleSearchWindow()
        {
            var searchWindow = CreateInstance<global::Waddle.SearchWindow.SearchWindow>();
            searchWindow.Items = GetModuleItems();
            searchWindow.OnSelection += item => AddModuleFromGuid((string)item.Content);
            searchWindow.ShowAsDropDown(default, WindowSize);
            searchWindow.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), WindowSize);
        }

        private List<SearchView.Item> GetModuleItems()
        {
            var type = typeof(ModuleDefinitionContainer);
            return AssetDatabase.FindAssets($"t:{type.Namespace}.{type.Name}")
                .Select(guid => new SearchView.Item
                {
                    Content = guid,
                    Path = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid))
                }).ToList();
        }

        private void AddModuleFromGuid(string guid)
        {
            var moduleDefinitionContainer = AssetDatabase.LoadAssetAtPath<ModuleDefinitionContainer>(AssetDatabase.GUIDToAssetPath(guid));
            
            _modulesProperty.InsertArrayElementAtIndex(_modulesProperty.arraySize);
            var module = EntityContainer.ModuleWrapper.FromModule(moduleDefinitionContainer.ToModule());
            _modulesProperty.GetArrayElementAtIndex(_modulesProperty.arraySize - 1).boxedValue = module;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            Repaint();
        }
    }
}