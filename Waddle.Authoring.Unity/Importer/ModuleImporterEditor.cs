using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Waddle.Authoring.Unity.Inspectors;

namespace Waddle.Authoring.Unity.Importer
{
    [CustomEditor(typeof(ModuleImporter))]
    public class ModuleImporterEditor : ScriptedImporterEditor
    {
        protected override Type extraDataType => typeof(ModuleDefinitionContainer);

        public override bool showImportedObject => false;

        protected override bool needsApplyRevert => false;

        public override void OnEnable()
        {
            base.OnEnable();
        }
        
        protected override void OnHeaderGUI()
        {
            CreateEditor(extraDataTarget).DrawHeader();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(CreateModuleInspector());

            root.Add(new IMGUIContainer(() =>
            {
                AddApplyRevertGUI();
            }));

            return root;
        }

        private VisualElement CreateModuleInspector()
        {
            var inspector = CreateEditor(extraDataTarget, typeof(ModuleInspector)).CreateInspectorGUI();
            inspector.Bind(extraDataSerializedObject);
            return inspector;
        }

        protected override void InitializeExtraDataInstance(UnityEngine.Object extraData, int targetIndex)
        {
            EditorUtility.CopySerialized(assetTargets[targetIndex], extraData);
        }

        protected override void Apply()
        {
            base.Apply();
            for (int i = 0; i < targets.Length; i++)
            {
                string path = ((AssetImporter)targets[i]).assetPath;
                var module = (ModuleDefinitionContainer)extraDataTargets[i];
                File.WriteAllText(path, module.ToJson());
            }
        }

        private void AddApplyRevertGUI()
        {
            this.hasUnsavedChanges = this.HasModified();
            this.extraDataSerializedObject.ApplyModifiedProperties();
            this.extraDataSerializedObject.Update();
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope(Array.Empty<GUILayoutOption>()))
            {
                GUILayout.FlexibleSpace();
                if (this.OnApplyRevertGUI())
                    GUIUtility.ExitGUI();
            }
        }
    }
}
