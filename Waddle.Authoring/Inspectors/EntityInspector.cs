using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Waddle.Authoring.Registry;
using Waddle.SearchWindow;

namespace Waddle.Authoring.Inspectors
{
    [CustomEditor(typeof(Entity))]
    public class EntityInspector : Editor
    {
        [SerializeField] private VisualTreeAsset _editorAsset;
        [SerializeField] private VisualTreeAsset _itemAsset;

        private static readonly Vector2 WindowSize = new Vector2(200, 400);

        public override bool UseDefaultMargins() => false;
        
        public override VisualElement CreateInspectorGUI()
        {
            var modulesProperty = serializedObject.FindProperty("_modules");
            
            var root = _editorAsset.CloneTree();
            root.Q<Button>("AddModuleButton").clicked += () => OpenModuleSearchWindow(modulesProperty);
            var listView = root.Q<ListView>();
            listView.makeItem = _itemAsset.CloneTree;
            listView.bindItem = (element, moduleIndex) => BindItem(element, modulesProperty, moduleIndex);

            return root;
        }

        private void BindItem(VisualElement element, SerializedProperty modulesProperty, int moduleIndex)
        {
            var moduleProperty = modulesProperty.GetArrayElementAtIndex(moduleIndex);
            
            var fieldList = element.Q<Foldout>();
            fieldList.AddManipulator(new ContextualMenuManipulator(menu =>
            {
                menu.menu.AppendAction("Remove Module", _ => RemoveModule(modulesProperty, moduleIndex));
            }));
            fieldList.text = ObjectNames.NicifyVariableName(moduleProperty.FindPropertyRelative("ModuleDefinition").objectReferenceValue.name);

            var fieldsProperty = moduleProperty.FindPropertyRelative("Fields");
            var fieldsRoot = fieldList.Q("FieldsRoot");
            fieldsRoot.Clear();
            for (int i = 0; i < fieldsProperty.arraySize; i++)
            {
                var fieldRoot = CreateFieldInspector(fieldsProperty.GetArrayElementAtIndex(i).objectReferenceValue);
                fieldsRoot.Add(fieldRoot);
            }
        }
        
        private VisualElement CreateFieldInspector(Object fieldObject)
        {
            var fieldRoot = new VisualElement();
            var field = new SerializedObject(fieldObject);
            InspectorElement.FillDefaultInspector(fieldRoot, field, this);
            fieldRoot.RemoveAt(0);
            if(fieldRoot.childCount == 1)
            {
                fieldRoot[0].Q<PropertyField>().label = field.targetObject.name;
            }
            fieldRoot.Bind(field);
            return fieldRoot;
        }

        private void RemoveModule(SerializedProperty modulesProperty, int moduleIndex)
        {
            modulesProperty.DeleteArrayElementAtIndex(moduleIndex);
            modulesProperty.serializedObject.ApplyModifiedProperties();
            Repaint();
        }
    
        private void OpenModuleSearchWindow(SerializedProperty modulesProperty)
        {
            var searchWindow = CreateInstance<Waddle.SearchWindow.SearchWindow>();
            searchWindow.Items = GetModuleItems();
            searchWindow.OnSelection += item => AddModuleFromGuid(modulesProperty, (string)item.Content);
            searchWindow.ShowAsDropDown(default, WindowSize);
            searchWindow.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), WindowSize);
        }

        private List<SearchView.Item> GetModuleItems()
        {
            var modules = ModuleRegistry.GetModulesFromEntity(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target)));
            var type = typeof(ModuleDefinition);
            return AssetDatabase.FindAssets($"t:{type.Namespace}.{type.Name}")
                .Where(guid => !modules.Contains(guid))
                .Select(guid => new SearchView.Item
                {
                    Content = guid,
                    Path = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid))
                }).ToList();
        }

        private void AddModuleFromGuid(SerializedProperty modulesProperty, string guid)
        {
            var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            modulesProperty.InsertArrayElementAtIndex(modulesProperty.arraySize);
            modulesProperty.GetArrayElementAtIndex(modulesProperty.arraySize - 1).boxedValue = new Entity.ModuleInstance()
            {
                Fields = new List<Field>(),
                ModuleDefinition = module,
            };
            modulesProperty.serializedObject.ApplyModifiedProperties();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            Repaint();
        }
    }
}