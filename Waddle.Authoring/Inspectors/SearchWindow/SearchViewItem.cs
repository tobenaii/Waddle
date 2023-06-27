using BovineLabs.Core.Editor.SearchWindow;
using UnityEngine;
using UnityEngine.UIElements;

namespace Waddle.Authoring.Inspectors.SearchWindow
{
    internal class SearchViewItem : VisualElement
    {
        [SerializeField] private VisualTreeAsset _searchItemTemplate;

        private readonly VisualElement icon;
        private readonly Label label;
        private readonly VisualElement nextIcon;

        public SearchViewItem()
        {
            this.AddToClassList("SearchItem");

            _searchItemTemplate.CloneTree(this);

            this.label = this.Q<Label>("Label");
            this.icon = this.Q("Icon");
            this.nextIcon = this.Q("NextIcon");
            this.tabIndex = -1;
        }

        public string Name { get; private set; }

        public TreeNode<SearchView.Item> Item
        {
            get => this.userData as TreeNode<SearchView.Item>;
            set
            {
                this.userData = value;
                this.icon.style.backgroundImage = value.Value.Icon;
                this.Name = value.Value.Name;
                this.label.text = this.Name;

                this.nextIcon.style.visibility = value.ChildCount == 0 ? Visibility.Hidden : Visibility.Visible;
            }
        }
    }
}
