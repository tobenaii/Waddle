using UnityEngine;

namespace Waddle.Authoring.Fields
{
    public class TextField : Field<string>
    {
        [SerializeField] private string _string;
        public override string Value => _string;
    }
}