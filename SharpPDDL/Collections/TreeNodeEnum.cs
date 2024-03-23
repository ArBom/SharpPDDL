using System;
using System.Collections.Generic;
using System.Text;

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

    internal class BrachNodeEnum<T> : TreeNodeEnum<T> where T : class
    {
        private readonly TreeNode<T> creator;

        internal BrachNodeEnum(TreeNode<T> creator) : base(creator)
        {
            this.creator = creator;
        }

        private new bool MoveNextFromLine(TreeNode<T> e)
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
                    if (e.Root.Equals(creator))
                        return false;

                    return MoveNextFromLine(e.Root);
                }
            }
            return false;
        }

        new public void Reset()
        {
            TreeNode<T> MinusOnePos = new TreeNode<T>
            {
                Children = new List<TreeNode<T>> { creator }
            };
            this.current = MinusOnePos;
        }
    }
}
