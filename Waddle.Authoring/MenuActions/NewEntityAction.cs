using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Waddle.Authoring.MenuActions
{
    public static class NewEntityAction
    {
        [MenuItem("Assets/Create/Waddle/Entity", false, -99)]
        private static void CreateNewModuleDefinition()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Entity.entity");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<CreateEntityAction>(), assetPathAndName, null, null, true);
        }
        
        private class CreateEntityAction : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                File.Create(pathName).Dispose();
                AssetDatabase.Refresh();
            }
        }
    }
}