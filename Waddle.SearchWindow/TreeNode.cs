using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Waddle.Waddle.SearchWindow
{
    internal class TreeNode<T>
    {
        internal List<TreeNode<T>> _children = new();

        public T Value { get; set; }

        public ReadOnlyCollection<TreeNode<T>> Children => _children.AsReadOnly();

        public int ChildCount => _children.Count;

        public TreeNode(T value)
        {
            Value = value;
        }

        public TreeNode<T> this[int i] => _children[i];

        public TreeNode<T> Parent { get; internal set; }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            _children.Add(node);
            return node;
        }

        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children) child.Traverse(action);
        }

        public void Traverse(Action<TreeNode<T>> action)
        {
            action(this);
            foreach (var child in _children) child.Traverse(action);
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(_children.SelectMany(x => x.Flatten()));
        }
    }
}