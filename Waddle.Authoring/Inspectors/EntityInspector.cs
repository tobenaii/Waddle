using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Search;
using UnityEngine.UIElements;

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
            var root = _editorAsset.CloneTree();
            var listView = root.Q<ListView>();
            listView.Q<Button>("unity-list-view__add-button").clickable = new Clickable(OpenModuleSearchWindow);
            listView.makeItem = _itemAsset.CloneTree;
            listView.bindItem = (element, moduleIndex) =>
            {
                var moduleProperty = serializedObject.FindProperty("_modules").GetArrayElementAtIndex(moduleIndex);
                
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

        private void OpenModuleSearchWindow()
        {
            SearchContext context = SearchService.CreateContext(GetProvider(), "", 
                SearchFlags.OpenPicker | SearchFlags.HidePanels);
            EditorWindow window = (EditorWindow)SearchService.ShowPicker(new SearchViewState(context, AddModule, null, "", typeof(ModuleDefinition))
            {
                excludeClearItem = true,
                hideTabs = true,
                hideAllGroup = true,
                flags = SearchViewFlags.Borderless | 
                        SearchViewFlags.CompactView | 
                        SearchViewFlags.DisableInspectorPreview | 
                        SearchViewFlags.DisableBuilderModeToggle |
                        SearchViewFlags.DisableSavedSearchQuery |
                        SearchViewFlags.NoIndexing
            });
            window.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(200, 400));
        }

        private void AddModule(Object module, bool cancelled)
        {
            if (cancelled) return;

            var entity = (Entity)target;
            var moduleInstance = entity.AddModuleInstance();
            moduleInstance.ModuleDefinition = (ModuleDefinition)module;
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(entity));
            Repaint();
        }

        private static IEnumerable<SearchProvider> GetProvider()
        {
            yield return SearchService.GetProvider("modules");
        }

        [SearchItemProvider]
        private static SearchProvider CreateProvider()
        {
            return new SearchProvider("modules", "Modules")
            {
                fetchItems = (context, items, provider) =>
                {
                    var type = typeof(ModuleDefinition);
                    var results = AssetDatabase.FindAssets($"t:{type.Namespace}.{type.Name}" + context.searchQuery);
                    foreach (var guid in results)
                    {
                        items.Add(provider.CreateItem(AssetDatabase.GUIDToAssetPath(guid), null, null, (Texture2D)EditorGUIUtility.IconContent("d_ScriptableObject Icon").image, null));
                    }
                    return null;
                },
                fetchLabel = (item, _) => Path.GetFileNameWithoutExtension(item.id),
                fetchPreview = (_, _, _, _) => (Texture2D)EditorGUIUtility.IconContent("d_ScriptableObject Icon").image,
                toObject = (item, _) => AssetDatabase.LoadMainAssetAtPath(item.id),
            };
        }
    }
}
