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
        public override bool UseDefaultMargins() => false;

        [SerializeField] private VisualTreeAsset _entityModuleContainer;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var modulesProperty = serializedObject.FindProperty("_modules");
            for (int i = 0; i < modulesProperty.arraySize; i++)
            {
                var moduleProperty = modulesProperty.GetArrayElementAtIndex(i);
                
                var moduleContainer = _entityModuleContainer.CloneTree(moduleProperty.propertyPath);

                var foldout = moduleContainer.Q<Foldout>();
                foldout.text = moduleProperty.FindPropertyRelative("Module").objectReferenceValue.name;

                var fieldsProperty = moduleProperty.FindPropertyRelative("Fields");
                var fieldsContainer = new VisualElement();
                for (int s = 0; s < fieldsProperty.arraySize; s++)
                {
                    var fieldContainer = new VisualElement();
                    DrawField(fieldContainer, new SerializedObject(fieldsProperty.GetArrayElementAtIndex(s).objectReferenceValue));
                    fieldsContainer.Add(fieldContainer);
                }
                foldout.Add(fieldsContainer);
                root.Add(moduleContainer);
            }

            var addModuleButton = new Button(() =>
            {
                OpenModuleSearchWindow();
            })
            {
                text = "Add Module",
            };
            root.Add(addModuleButton);
            return root;
        }

        private void OpenModuleSearchWindow()
        {
            SearchContext context = SearchService.CreateContext(GetProvider(), "", 
                SearchFlags.OpenPicker | SearchFlags.HidePanels);
            EditorWindow window = (EditorWindow)SearchService.ShowPicker(new SearchViewState(context, AddModule, null, "", typeof(Module))
            {
                excludeClearItem = true,
                hideTabs = true,
                hideAllGroup = true,
                flags = SearchViewFlags.Borderless | 
                        SearchViewFlags.CompactView | 
                        SearchViewFlags.DisableQueryHelpers | 
                        SearchViewFlags.DisableInspectorPreview | 
                        SearchViewFlags.DisableBuilderModeToggle |
                        SearchViewFlags.DisableSavedSearchQuery |
                        SearchViewFlags.DisableNoResultTips |
                        SearchViewFlags.NoIndexing
            });
            window.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(200, 400));
        }

        private void AddModule(Object module, bool cancelled)
        {
            if (cancelled) return;

            var entity = (Entity)target;
            var moduleInstance = entity.AddModuleInstance();
            moduleInstance.Module = (Module)module;
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
                    var type = typeof(Module);
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

        private static void DrawField(VisualElement container, SerializedObject serializedObject)
        {
            var foldout = new Foldout();
            foldout.text = serializedObject.targetObject.name;
            var property = serializedObject.GetIterator();
            if (!property.NextVisible(true)) return; // Expand first child.
            do
            {
                if (property.propertyPath is "m_Script" or "ID")
                {
                    continue;
                }

                var field = new PropertyField(property)
                {
                    name = "PropertyField:" + property.propertyPath
                };


                if (property.propertyPath == "m_Script" && serializedObject.targetObject != null)
                {
                    field.SetEnabled(false);
                }

                foldout.Add(field);
            } while (property.NextVisible(false));
            container.Bind(serializedObject);
            container.Add(foldout);
        }
    }
}
