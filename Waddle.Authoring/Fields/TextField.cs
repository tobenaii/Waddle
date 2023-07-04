using UnityEngine;
using UnityEngine.Localization;

namespace Waddle.Authoring.Fields
{
    public class TextField : Field<long>
    {
        [SerializeField] private LocalizedString _string;
        public override long Value => _string.TableEntryReference;
    }
}