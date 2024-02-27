using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    internal class TreeNode<T> where T : class
    {
        public T Content;
        internal TreeNode<T> Root;
        internal List<TreeNode<T>> Children;

        internal TreeNode(T Content)
        {
            this.Root = null;
            this.Children = new List<TreeNode<T>>();

            this.Content = Content;
        }

        internal void ChangeParentIntoGrandpa(ref TreeNode<T> newParent)
        {
            int index = this.Root.Children.IndexOf(this);
            newParent.Root = this.Root;
            newParent.Children.Add(this);
            this.Root.Children[index] = newParent;
            this.Root = newParent;
        }
    }
}
