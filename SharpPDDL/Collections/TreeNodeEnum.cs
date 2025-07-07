using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class TreeNodeEnum<T> : IEnumerator<T> where T : class
    {
        protected TreeNode<T> current;

        internal TreeNodeEnum(TreeNode<T> creator)
        {
            TreeNode<T> MinusOnePos = new TreeNode<T>
            {
                Children = new List<TreeNode<T>> { creator }
            };
            this.current = MinusOnePos;
        }

        public object Current => current;

        T IEnumerator<T>.Current => current.Content;

        protected bool MoveNextFromLine(TreeNode<T> e)
        {
            if (!(e.Root is null))
            {
                int IndeksOfE = e.Root.Children.IndexOf(e);

                if (e.Root.Children.Count != ++IndeksOfE)
                {
                    e = e.Root.Children[IndeksOfE];
                    return true;
                }
                else
                {
                    return MoveNextFromLine(e.Root);
                }
            }
            return false;
        }

        public bool MoveNext()
        {
            if (current.Children.Count != 0)
            {
                current = current.Children[0];
                return true;
            }

            return MoveNextFromLine(current);
        }

        public void Reset()
        {
            while (current.Root != null)
            {
                current = current.Root;
            }
            TreeNode<T> MinusOnePos = new TreeNode<T>
            {
                Children = new List<TreeNode<T>> { current }
            };
            this.current = MinusOnePos;
        }

        void IDisposable.Dispose() { }
    }
}
