using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    internal struct CrisscrossChildrenCon
    { 
        internal Crisscross Child;
        internal int ActionNr;
        internal object[] ActionArgOryg;

        internal CrisscrossChildrenCon(Crisscross Child, int ActionNr, object[] ActionArgOryg)
        {
            this.Child = Child;
            this.ActionNr = ActionNr;
            this.ActionArgOryg = ActionArgOryg;
        }
    }

    internal class Crisscross : ICollection
    {
        public PossibleState Content;
        public Crisscross Root;
        public List<Crisscross> AlternativeRoots;
        public List<CrisscrossChildrenCon> Children;
        public List<int> CheckedAction;
        public UInt32 CumulativedTransitionCharge { get; private set; }

        internal Crisscross()
        {
            this.Root = null;
            this.AlternativeRoots = new List<Crisscross>();
            this.Children = new List<CrisscrossChildrenCon>();
            this.CheckedAction = new List<int>();
            this.Content = null;
            this.CumulativedTransitionCharge = 0;
        }

        public void Add(PossibleState item) => this.Add(item, 0, new object[0], 1);

        public Crisscross Add(PossibleState item, int ActionNr, object[] ActionArg, UInt32 AddedTransitionCharge)
        {
            if (AddedTransitionCharge == 0)
                AddedTransitionCharge = 1;

            Crisscross AddedItem = new Crisscross()
            {
                Root = this,
                Content = item,
                CumulativedTransitionCharge = this.CumulativedTransitionCharge + AddedTransitionCharge
            };

            this.Children.Add(new CrisscrossChildrenCon(AddedItem, ActionNr, ActionArg));
            return AddedItem;
        }

        public Crisscross this[int key]
        {
            get { return this.Children[key].Child; }
        }

        public bool Contains(PossibleState item)
        {
            if (this.Content.Equals(item))
                return true;

            foreach (var child in this.Children)
                if (child.Child.Contains(item))
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return new CrisscrossEnum(this);
        }

        public bool IsReadOnly => false;

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        private List<int> Position(List<int> previesly)
        {
            if (this.Root == null)
                return previesly;

            var thisOfRoot = this.Root.Children.First(c => c.Child == this);
            List<int> current = new List<int>(this.Root.Children.IndexOf(thisOfRoot));
            current.AddRange(previesly);
            return this.Root.Position(current);
        }

        public List<int> Position()
        {
            if (this.Root == null)
                return null;

            var thisOfRoot = this.Root.Children.First(c => c.Child == this);
            List<int> ToRet = new List<int>(this.Root.Children.IndexOf(thisOfRoot));

            return Position(ToRet);
        }

        private void MoveNodesToList(Crisscross node, List<PossibleState> resultList)
        {
            resultList.Add(node.Content);

            foreach (var child in node.Children)
                MoveNodesToList(child.Child, resultList);
        }

        public PossibleState[] ToArray()
        {
            List<PossibleState> resultList = new List<PossibleState>();
            MoveNodesToList(this, resultList);
            return resultList.ToArray();
        }

        public List<PossibleState> ToList()
        {
            List<PossibleState> resultList = new List<PossibleState>();
            MoveNodesToList(this, resultList);
            resultList = resultList.Distinct().ToList();
            return resultList;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(PossibleState item)
        {
            throw new NotImplementedException();
        }

        public void Merge(Crisscross MergeWith)
        {
            throw new NotImplementedException();

            if (MergeWith.CumulativedTransitionCharge < this.CumulativedTransitionCharge)
            {

            }
        }

        public void CopyTo(Array array, int index)
        {
            ToArray().CopyTo(array, index);
        }
    }
}
