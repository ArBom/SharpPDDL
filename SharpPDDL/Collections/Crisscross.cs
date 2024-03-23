using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    internal class Crisscross<T> : ICollection<T> where T : class
    {
        public T Content;
        public Crisscross<T> Root;
        public List<Crisscross<T>> AlternativeRoots;
        public List<Crisscross<T>> Children;
        public UInt32 CumulativedTransitionCharge { get; private set; }

        internal Crisscross()
        {
            this.Root = null;
            this.AlternativeRoots = new List<Crisscross<T>>();
            this.Children = new List<Crisscross<T>>();
            this.Content = null;
            this.CumulativedTransitionCharge = 0;
        }

        public void Add(T item) => this.Add(item, 1);

        public void Add(T item, UInt32 AddedTransitionCharge)
        {
            if (AddedTransitionCharge == 0)
                AddedTransitionCharge = 1;

            Crisscross<T> AddedItem = new Crisscross<T>()
            {
                Root = this,
                Content = item,
                CumulativedTransitionCharge = this.CumulativedTransitionCharge + AddedTransitionCharge
            };

            this.Children.Add(AddedItem);
        }

        public Crisscross<T> this[int key]
        {
            get { return this.Children[key]; }
            set { this.Children[key] = value; }
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
                return this.ToList().Count;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ToArray().CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new CrisscrossEnum<T>(this);
        }

        public bool IsReadOnly => false;

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

        private void MoveNodesToList(Crisscross<T> node, List<T> resultList)
        {
            resultList.Add(node.Content);

            foreach (Crisscross<T> child in node.Children)
                MoveNodesToList(child, resultList);
        }

        public T[] ToArray()
        {
            List<T> resultList = new List<T>();
            MoveNodesToList(this, resultList);
            return resultList.ToArray();
        }

        public List<T> ToList()
        {
            List<T> resultList = new List<T>();
            MoveNodesToList(this, resultList);
            resultList = resultList.Distinct().ToList();
            return resultList;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }
    }
}
