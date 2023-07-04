using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Waddle.SearchWindow
{
    public class SearchWindow : EditorWindow
    {
        private SearchView _searchView;
        
        public List<SearchView.Item> Items {
            get => _searchView.Items;
            set => _searchView.Items = value;
        }
        
        public event Action<SearchView.Item> OnSelection;
        
        public string Title {
            get => _searchView.Title;
            set {
                _searchView.Title = value;
            }
        }

        private void OnEnable()
        {
            _searchView = new SearchView();
            rootVisualElement.Add(_searchView);
            rootVisualElement.style.color = Color.white;
            _searchView.OnSelection += (e) =>
            {
                OnSelection?.Invoke(e);
                Close();
            };
        }

        public static SearchWindow Create()
        {
            SearchWindow window = EditorWindow.CreateInstance<SearchWindow>();

            return window;
        }


        private void OnFocus()
        {
            _searchView.Q<SearchField>().Q("unity-text-input").Focus();
        }
    }
}
