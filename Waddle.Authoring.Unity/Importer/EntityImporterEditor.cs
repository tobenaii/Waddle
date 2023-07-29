using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Waddle.Authoring.Unity.Inspectors;

namespace Waddle.Authoring.Unity.Importer
{
    [CustomEditor(typeof(EntityImporter))]
    public class EntityImporterEditor : ScriptedImporterEditor
    {
        protected override Type extraDataType => typeof(EntityContainer);

        public override bool showImportedObject => false;

        protected override bool needsApplyRevert => false;

        private List<SerializedObject> _fieldObjects = new();

        protected override void OnHeaderGUI()
        {
            CreateEditor(extraDataTarget).DrawHeader();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(CreateModuleInspector());
            
            root.Add(new IMGUIContainer(AddApplyRevertGUI));

            return root;
        }

        private VisualElement CreateModuleInspector()
        {
            var inspector = CreateEditor(extraDataTarget, typeof(EntityInspector)).CreateInspectorGUI();
            inspector.Bind(extraDataSerializedObject);
            return inspector;
        }

        protected override void InitializeExtraDataInstance(UnityEngine.Object extraData, int targetIndex)
        {
            var filePath = ((AssetImporter)targets[targetIndex]).assetPath;
            extraData.name = Path.GetFileNameWithoutExtension(filePath);

            var entityContainer = (EntityContainer)extraData;
            entityContainer.FromJson(File.ReadAllText(filePath));

            RefreshFieldObjects();
        }

        protected override void Apply()
        {
            base.Apply();
            for (int i = 0; i < targets.Length; i++)
            {
                string path = ((AssetImporter)targets[i]).assetPath;
                var module = (EntityContainer)extraDataTargets[i];
                File.WriteAllText(path, module.ToJson());
            }
            RefreshFieldObjects();
        }

        public override void DiscardChanges()
        {
            base.DiscardChanges();
            foreach (var field in _fieldObjects.ToList())
            {
                var currentField = new SerializedObject(field.targetObject);
                    
                SerializedProperty oldProperty = field.GetIterator();
            
                bool enterChildren = true;
                while (oldProperty.NextVisible(enterChildren)) {
                    currentField.CopyFromSerializedProperty(oldProperty);
                    enterChildren = false;
                }
                    
                currentField.ApplyModifiedProperties();
            }
        }

        private void RefreshFieldObjects()
        {
            _fieldObjects.Clear();
            
            var entityContainer = (EntityContainer)extraDataTarget;
            foreach (var module in entityContainer.Modules)
            {
                foreach (var field in module.Fields)
                {
                    var serializedField = new SerializedObject(field.Value);
                    serializedField.Update();
                    _fieldObjects.Add(serializedField);
                }
            }
        }

        private bool HasFieldModified()
        {
            foreach (var field in _fieldObjects)
            {
                bool modified = ComparisonCheck(field, new SerializedObject(field.targetObject));
                if (modified)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ComparisonCheck(SerializedObject obj1, SerializedObject obj2)
        {
            SerializedProperty oldProperty = obj1.GetIterator();
            SerializedProperty newProperty = obj2.GetIterator();
            
            bool enterChildren = true;
            while (oldProperty.NextVisible(enterChildren) && newProperty.NextVisible(enterChildren)) {
                if (!SerializedProperty.DataEquals(oldProperty, newProperty))
                {
                    return true;
                }
                enterChildren = false;
            }
            return false;
        }

        private void AddApplyRevertGUI()
        {
            hasUnsavedChanges = HasModified();
            if (!hasUnsavedChanges)
            {
                hasUnsavedChanges = HasFieldModified();
            }
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
            {
                GUILayout.FlexibleSpace();
                if (OnApplyRevertGUI())
                    GUIUtility.ExitGUI();
            }
        }
    }
}
