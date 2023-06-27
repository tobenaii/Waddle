using UnityEngine;

namespace Waddle.Authoring.Fields
{
    public class FloatField : Field<float>
    {
        [SerializeField] private float _value;
        public override float Value => _value;
    }
}