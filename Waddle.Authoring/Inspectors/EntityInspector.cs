using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Search;
using UnityEngine.UIElements;
using Waddle.SearchWindow;

namespace Waddle.Authoring.Inspectors
{
    [CustomEditor(typeof(Entity))]
    public class EntityInspector : Editor
    {
        [SerializeField] VisualTreeAsset _editorAsset;
        [SerializeField] VisualTreeAsset _itemAsset;

        public override bool UseDefaultMargins() => false;
        public override VisualElement CreateInspectorGUI()
        {
            var modulesProperty = serializedObject.FindProperty("_modules");
            
            var root = _editorAsset.CloneTree();
            root.Q<Button>("AddModuleButton").clicked += () => OpenModuleSearchWindow(modulesProperty);
            var listView = root.Q<ListView>();
            listView.makeItem = _itemAsset.CloneTree;
            listView.bindItem = (element, moduleIndex) =>
            {
                InstallRemoveManipulator(element.Q<Foldout>(), modulesProperty, moduleIndex);
                
                var moduleProperty = modulesProperty.GetArrayElementAtIndex(moduleIndex);
                
                var fieldList = element.Q<Foldout>();
                fieldList.text = ObjectNames.NicifyVariableName(moduleProperty.FindPropertyRelative("ModuleDefinition").objectReferenceValue.name);
                
                var fieldsProperty = moduleProperty.FindPropertyRelative("Fields");
                var fieldsRoot = fieldList.Q("FieldsRoot");
                fieldsRoot.Clear();
                for (int i = 0; i < fieldsProperty.arraySize; i++)
                {
                    var fieldRoot = new VisualElement();
                    var field = new SerializedObject(fieldsProperty.GetArrayElementAtIndex(i).objectReferenceValue);
                    InspectorElement.FillDefaultInspector(fieldRoot, field, this);
                    fieldRoot.RemoveAt(0);
                    if (fieldRoot.childCount == 1)
                    {
                        fieldRoot[0].Q<PropertyField>().label = field.targetObject.name;
                    }
                    fieldRoot.Bind(field);
                    fieldsRoot.Add(fieldRoot);
                }
            };
            return root;
        }

        private void InstallRemoveManipulator(VisualElement element, SerializedProperty modulesProperty, int moduleIndex)
        {
            var manipulator = new ContextualMenuManipulator(menu =>
            {
                menu.menu.AppendAction("Remove Module", _ =>
                {
                    modulesProperty.DeleteArrayElementAtIndex(moduleIndex);
                    modulesProperty.serializedObject.ApplyModifiedProperties();
                    Repaint();
                });
            });
            manipulator.target = element;
        }

        private void OpenModuleSearchWindow(SerializedProperty modulesProperty)
        {
            var searchWindow = CreateInstance<Waddle.SearchWindow.SearchWindow>();

            var type = typeof(ModuleDefinition);
            searchWindow.Items = AssetDatabase.FindAssets($"t:{type.Namespace}.{type.Name}").Select(guid =>
                new SearchView.Item()
                {
                    Content = guid,
                    Path = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid))
                }).ToList();
            searchWindow.OnSelection += item =>
            {
                var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(AssetDatabase.GUIDToAssetPath((string)item.Content));
                modulesProperty.InsertArrayElementAtIndex(modulesProperty.arraySize);
                modulesProperty.GetArrayElementAtIndex(modulesProperty.arraySize - 1).boxedValue = new Entity.ModuleInstance()
                {
                    Fields = new List<Field>(),
                    ModuleDefinition = module,
                };
                modulesProperty.serializedObject.ApplyModifiedProperties();
                
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                Repaint();
            };
            searchWindow.ShowAsDropDown(default, new Vector2(200, 400));
            searchWindow.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(200, 400));
        }
    }
}
