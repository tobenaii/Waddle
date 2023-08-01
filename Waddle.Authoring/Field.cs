namespace Waddle.Authoring
{
    [System.Serializable]
    public class Field
    {
        public string Name;
        public string TypeID;
        public string FieldID;
        public IFieldValue Value;
    }

    public interface IFieldValue
    {
        public string Serialize();
        public void Deserialize(string json);
    }
}