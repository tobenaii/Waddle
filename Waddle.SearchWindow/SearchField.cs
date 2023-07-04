using UnityEngine;
using UnityEngine.UIElements;

namespace Waddle.SearchWindow
{
    class SearchField : TextField
    {
        VisualElement _searchContainer;

        internal new class UxmlFactory : UxmlFactory<SearchField, UxmlTraits> { }

        public SearchField()
        {
            LoadLayout();
        }

        public SearchField(string label) : base(label)
        {
            LoadLayout();
        }

        public SearchField(int maxLength, bool multiline, bool isPasswordField, char maskChar) : base(maxLength, multiline, isPasswordField, maskChar)
        {
            LoadLayout();
        }
        public SearchField(string label, int maxLength, bool multiline, bool isPasswordField, char maskChar) : base(label, maxLength, multiline, isPasswordField, maskChar)
        {
            LoadLayout();
        }

        void LoadLayout()
        {
            styleSheets.Add(Resources.Load("SearchFieldStyles") as StyleSheet);
            var visualTree = Resources.Load("SearchFieldLayout") as VisualTreeAsset;
            visualTree.CloneTree(this);

            _searchContainer = this.Q<VisualElement>(null, "search-field__container");

            RegisterCallback<FocusInEvent>(e =>
            {
                _searchContainer.style.display = DisplayStyle.None;
            });

            RegisterCallback<FocusOutEvent>(e =>
            {
                _searchContainer.style.display = (value.Length == 0) ? DisplayStyle.Flex : DisplayStyle.None;
            });
        }
    }
}