using System.Collections.Generic;

namespace Waddle.Authoring.Core
{
    [System.Serializable]
    public class ModuleDefinition
    {
        public string Name;
        public string ModuleID;
        public List<FieldDefinition> FieldDefinitions;
    }
}