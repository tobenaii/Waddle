using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIExtras;

namespace Waddle.Waddle.SearchWindow
{
    class SearchView : VisualElement
    {
        public event Action<Item> OnSelection;

        private readonly Button _returnButton;
        private readonly VisualElement _returnIcon;
        private readonly ListView _list;

        public List<Item> Items
        {
            get => m_Items;
            set
            {
                m_Items = value;
                Reset();
            }
        }

        public struct Item
        {
            public string Path;
            public Texture2D Icon;
            public string Name => System.IO.Path.GetFileName(Path);
        }

        List<Item> m_Items;

        string m_Title;

        public string Title
        {
            get => m_Title;
            set
            {
                m_Title = value;
                RefreshTitle();
            }
        }

        private TreeNode<Item> _rootNode;
        private TreeNode<Item> _currentNode;
        private TreeNode<Item> _searchNode;

        public SelectionType SelectionType
        {
            get => _list.selectionType;
            set { _list.selectionType = value; }
        }

        public SearchView()
        {
            AddToClassList("SearchView");
            AddToClassList(EditorGUIUtility.isProSkin ? "UnityThemeDark" : "UnityThemeLight");

            styleSheets.Add(Resources.Load<StyleSheet>("SearchViewStyle"));
            var visualTree = Resources.Load<VisualTreeAsset>("SearchView");
            visualTree.CloneTree(this);

            var searchField = this.Q<SearchField>();
            _returnButton = this.Q<Button>("ReturnButton");
            _returnButton.clicked += OnNavigationReturn;
            _returnIcon = this.Q("ReturnIcon");
            _list = this.Q<ListView>("SearchResults");
            _list.selectionType = SelectionType.Single;
            _list.makeItem = () => new SearchViewItem();
            _list.bindItem = (element, index) =>
            {
                SearchViewItem searchItem = element as SearchViewItem;
                searchItem!.Item = _currentNode[index];
            };

            _list.selectionChanged += OnListSelectionChange;
            _list.itemsChosen += OnItemsChosen;

            Title = "Root";

            searchField.RegisterValueChangedCallback(OnSearchQueryChanged);
        }

        void OnSearchQueryChanged(ChangeEvent<string> changeEvent)
        {
            if (_searchNode != null && _currentNode == _searchNode)
            {
                _currentNode = _searchNode.Parent;
                _searchNode = null;
                if (changeEvent.newValue.Length == 0)
                {
                    SetCurrentSelectionNode(_currentNode);
                    return;
                }
            }

            if (changeEvent.newValue.Length == 0)
            {
                return;
            }

            var searchResults = new List<TreeNode<Item>>();
            _rootNode.Traverse(delegate(TreeNode<Item> itemNode)
            {
                if (itemNode.Value.Name.IndexOf(changeEvent.newValue, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    searchResults.Add(itemNode);
                }
            });
            _searchNode = new TreeNode<Item>(new Item { Path = "Search" });
            _searchNode._children = searchResults;
            _searchNode.Parent = _currentNode;
            SetCurrentSelectionNode(_searchNode);

        }

        void OnListSelectionChange(IEnumerable<object> selection)
        {
            if (SelectionType == SelectionType.Single)
            {
                OnItemsChosen(selection);
            }
            else
            {
                // TBD.
            }
        }

        void OnItemsChosen(IEnumerable<object> selection)
        {
            TreeNode<Item> node = selection.First() as TreeNode<Item>;
            if (node!.ChildCount == 0)
            {
                OnSelection?.Invoke(node.Value);
            }
            else
            {
                SetCurrentSelectionNode(node);
            }
        }

        void RefreshTitle()
        {
            if (_rootNode != null)
            {
                _rootNode.Value = new Item { Path = m_Title, Icon = null };
            }

            if (_currentNode == null)
            {
                _returnButton.text = m_Title;
                return;
            }

            _returnButton.text = _currentNode.Value.Name;
        }

        public void Reset()
        {
            _rootNode = new TreeNode<Item>(new Item { Path = m_Title, Icon = null });
            for (int i = 0; i < m_Items.Count; ++i)
            {
                Add(m_Items[i]);
            }

            SetCurrentSelectionNode(_rootNode);
        }

        void SetCurrentSelectionNode(TreeNode<Item> node)
        {
            _currentNode = node;
            _list.itemsSource = _currentNode.Children;
            _returnButton.text = _currentNode.Value.Name;
            if (node.Parent == null)
            {
                _returnButton.SetEnabled(false);
                _returnIcon.style.visibility = Visibility.Hidden;
            }
            else
            {
                _returnButton.SetEnabled(true);
                _returnIcon.style.visibility = Visibility.Visible;
            }

            _list.RefreshItems();
        }

        void OnNavigationReturn()
        {
            if (_currentNode != null && _currentNode.Parent != null)
            {
                SetCurrentSelectionNode(_currentNode.Parent);
            }
        }

        void Add(Item item)
        {
            if (item.Path.Length == 0)
            {
                return;
            }

            string[] pathParts = item.Path.Split('/');
            TreeNode<Item> parent = _rootNode;
            string currentPath = string.Empty;
            for (int i = 0; i < pathParts.Length; ++i)
            {
                if (currentPath.Length == 0)
                {
                    currentPath += pathParts[i];
                }
                else
                {
                    currentPath += "/" + pathParts[i];
                }

                TreeNode<Item> node = FindNodeByPath(parent, currentPath);
                if (node == null)
                {
                    node = parent.AddChild(new Item { Path = currentPath, Icon = null });
                }

                if (i == (pathParts.Length - 1))
                {
                    node.Value = item;
                }
                else
                {
                    parent = node;
                }
            }
        }

        TreeNode<Item> FindNodeByPath(TreeNode<Item> parent, string path)
        {
            if (parent == null || path.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < parent.ChildCount; ++i)
            {
                if (parent[i].Value.Path.Equals(path))
                {
                    return parent[i];
                }
            }

            return null;
        }
    }
}
