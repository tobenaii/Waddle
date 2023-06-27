using UnityEngine;
using UnityEngine.Localization;

namespace Waddle.Authoring.Fields
{
    public class TextField : Field<long>
    {
        [SerializeField] private LocalizedString _localizedString;
        public override long Value => _localizedString.TableEntryReference;
    }
}