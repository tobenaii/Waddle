using System.Collections.Generic;

namespace Waddle.Authoring
{
    [System.Serializable]
    public class Module
    {
        public string Name;
        public List<Field> Fields;
        public string ModuleID;
    }
}