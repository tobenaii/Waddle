using UnityEngine;
using UnityEngine.UIElements;

namespace Waddle.Authoring.Inspectors.SearchWindow
{
    internal class SearchField : TextField
    {
        [SerializeField] private VisualTreeAsset _searchFieldTemplate;

        private VisualElement _searchContainer;

        public SearchField()
        {
            this.LoadLayout();
        }

        public SearchField(string label)
            : base(label)
        {
            this.LoadLayout();
        }

        public SearchField(int maxLength, bool multiline, bool isPasswordField, char maskChar)
            : base(maxLength, multiline, isPasswordField, maskChar)
        {
            this.LoadLayout();
        }

        public SearchField(string label, int maxLength, bool multiline, bool isPasswordField, char maskChar)
            : base(label, maxLength, multiline, isPasswordField, maskChar)
        {
            this.LoadLayout();
        }

        private void LoadLayout()
        {
            _searchFieldTemplate.CloneTree(this);

            this._searchContainer = this.Q<VisualElement>(null, "search-field__container");

            this.RegisterCallback<FocusInEvent>(_ => { this._searchContainer.style.display = DisplayStyle.None; });

            this.RegisterCallback<FocusOutEvent>(_ => { this._searchContainer.style.display = this.value.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None; });
        }

        internal new class UxmlFactory : UxmlFactory<SearchField, UxmlTraits>
        {
        }
    }
}
