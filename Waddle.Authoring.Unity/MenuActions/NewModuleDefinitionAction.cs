using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Waddle.Authoring.Unity.MenuActions
{
    public static class NewModuleDefinitionAction
    {
        [MenuItem("Assets/Create/Waddle/Module Definition", false, -100)]
        private static void CreateNewModuleDefinition()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Module Definition.module");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateModuleDefinitionAction>(), assetPathAndName, null, null, true);
        }
        
        private class CreateModuleDefinitionAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                File.Create(pathName).Dispose();
                AssetDatabase.Refresh();
            }
        }
    }
}