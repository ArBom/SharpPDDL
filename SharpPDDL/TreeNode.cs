using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    internal class TreeNode<T> : ICollection<T> where T : class
    {
        public T Content;
        public TreeNode<T> Root;
        public List<TreeNode<T>> Children;

        internal TreeNode()
        {
            this.Root = null;
            this.Children = new List<TreeNode<T>>();
            this.Content = null;
        }

        public void Add(T item)
        {
            TreeNode<T> AddedItem = new TreeNode<T>()
            {
                Root = this,
                Content = item
            };

            this.Children.Add(AddedItem);
        }

        public void Clear()
        {
            foreach (var child in Children)
                child.Root = null;

            this.Children = null;

            if (!(this.Root is null))
            {
                this.Root.Children.Remove(this);
                this.Root = null;
            }
        }

        public bool Contains(T item)
        {
            if (this.Content.Equals(item))
                return true;

            foreach (var child in this.Children)
                if (child.Contains(item))
                    return true;

            return false;
        }

        public int Count
        {
            get
            {
                int count = 1;
                foreach (var child in this.Children)
                {
                    count = count + child.Count;
                }
                return count;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this.Root is null)
                return new TreeNodeEnum<T>(this);

            return new BrachNodeEnum<T>(this);
        }

        public bool IsReadOnly => false;

        public bool Remove(T item)
        {
            if (this.Content.Equals(item))
                return this.Remove();

            foreach (var child in this.Children)
                if (child.Remove(item))
                    return true;

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public TreeNode<T> this[int key]
        {
            get { return this.Children[key]; }
            set { this.Children[key] = value; }
        }

        private List<int> Position(List<int> previesly)
        {
            if (this.Root == null)
                return previesly;

            List<int> current = new List<int>(this.Root.Children.IndexOf(this));
            current.AddRange(previesly);
            return this.Root.Position(current);
        }

        public List<int> Position()
        {
            if (this.Root == null)
                return null;

            List<int> ToRet = new List<int>(this.Root.Children.IndexOf(this));

            return Position(ToRet);
        }

        internal void AddAbove(TreeNode<T> newParent)
        {
            int index = this.Root.Children.IndexOf(this);
            newParent.Root = this.Root;
            newParent.Children.Add(this);
            this.Root.Children[index] = newParent;
            this.Root = newParent;
        }

        private void MoveNodesToList(TreeNode<T> node, List<T> resultList)
        {
            resultList.Add(node.Content);

            foreach (TreeNode<T> child in node.Children)
                MoveNodesToList(child, resultList);
        }

        public List<T> ToList()
        {
            List<T> resultList = new List<T>();
            MoveNodesToList(this, resultList);
            return resultList;
        }

        public T[] ToArray()
        {
            List<T> resultList = new List<T>();
            MoveNodesToList(this, resultList);
            return resultList.ToArray();
        }

        public bool Remove()
        {
            if (this.Root is null)
                return false;

            for (int childCount = this.Children.Count; childCount >= 0; childCount--)
            {
                this.Root.Children.Add(this.Children[childCount]);
                this.Children[childCount].Root = this.Root;
            }

            this.Root.Children.Remove(this);
            this.Root = null;

            return true;
        }
    }

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