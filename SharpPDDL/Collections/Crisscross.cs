using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SharpPDDL
{
    internal class CrisscrossChildrenCon
    { 
        internal Crisscross Child;
        internal readonly int ActionNr;
        internal readonly object[] ActionArgOryg;

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
        public UInt32 CumulativedTransitionCharge { get; private set; }

        internal Crisscross()
        {
            this.Root = null;
            this.AlternativeRoots = new List<Crisscross>();
            this.Children = new List<CrisscrossChildrenCon>();
            this.Content = null;
            this.CumulativedTransitionCharge = 0;
        }

        public void Add(PossibleState item) => this.Add(item, 0, new object[0], 1, out Crisscross C);

        public void Add(PossibleState item, int ActionNr, object[] ActionArg, UInt32 AddedTransitionCharge, out Crisscross AddedItem)
        {
            if (AddedTransitionCharge == 0)
                AddedTransitionCharge = 1;

            AddedItem = new Crisscross()
            {
                Root = this,
                Content = item,
                CumulativedTransitionCharge = this.CumulativedTransitionCharge + AddedTransitionCharge
            };

            this.Children.Add(new CrisscrossChildrenCon(AddedItem, ActionNr, ActionArg));
            //TODO zobaczyć czy to to krzaczy mergowanie
            //Children.Sort((a, b) => a.Child.Content.GetHashCode().CompareTo(b.Child.Content.GetHashCode())); 
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

        private List<CrisscrossChildrenCon> Position(List<CrisscrossChildrenCon> previesly)
        {
            if (this.Root == null)
                return previesly;

            CrisscrossChildrenCon thisOfRoot = this.Root.Children.First(c => c.Child == this);
            List<CrisscrossChildrenCon> current = new List<CrisscrossChildrenCon>();
            current.Add(thisOfRoot);
            current.AddRange(previesly);
            return this.Root.Position(current);
        }

        public List<CrisscrossChildrenCon> Position()
        {
            if (this.Root == null)
                return null;

            var thisOfRoot = this.Root.Children.First(c => c.Child == this);
            List<CrisscrossChildrenCon> ToRet = new List<CrisscrossChildrenCon>();

            return Position(ToRet);
        }

        private void MoveNodesToList(Crisscross node, List<PossibleState> resultList)
        {
            resultList.Add(node.Content);

            foreach (var child in node.Children)
                if (!(child.Child.Root is null))
                    if (child.Child.Root.Equals(node))
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

        internal static void MergeK(ref Crisscross Incorporating, ref Crisscross Annexed)
        {
            //uint costDiff = Annexed.CumulativedTransitionCharge - Incorporating.CumulativedTransitionCharge;
            Annexed.AlternativeRoots.Add(Annexed.Root);

            //for every Alternative root of Annexed...
            for (int AnnAltRootI = 0; AnnAltRootI != Annexed.AlternativeRoots.Count; AnnAltRootI++)
            {
                //Zmiana childrena
                for (int i = 0; i != Annexed.AlternativeRoots[AnnAltRootI].Children.Count; i++)
                {
                    if (Annexed.AlternativeRoots[AnnAltRootI].Children[i].Child.Equals(Annexed))
                    {
                        Annexed.AlternativeRoots[AnnAltRootI].Children[i].Child = Incorporating;
                    }
                }

                //...add it to Incorporated AlternativeRoots
                bool IncorporatingAltRoorInclude = false;
                for(int IncoAltRootI = 0; IncoAltRootI != Incorporating.AlternativeRoots.Count; IncoAltRootI++)
                {
                    if(Incorporating.AlternativeRoots[IncoAltRootI].Equals(Annexed.AlternativeRoots[AnnAltRootI]))
                    {
                        IncorporatingAltRoorInclude = true;
                        break;
                    }
                }
                if(!IncorporatingAltRoorInclude)
                {
                    if (Incorporating.Root is null)
                    {
                        Incorporating.AlternativeRoots.Add(Annexed.AlternativeRoots[AnnAltRootI]);
                    }
                    else if (!Incorporating.Root.Equals(Annexed.AlternativeRoots[AnnAltRootI]))
                    {
                        Incorporating.AlternativeRoots.Add(Annexed.AlternativeRoots[AnnAltRootI]);
                    }
                }
                else
                {
                    int BreakPoint = 0;
                }
            }
                       
            foreach (var child in Annexed.Children)
            {
                if (child.Child.Root.Equals(Annexed))
                    child.Child.Root = Incorporating;
                else
                    for (int i = 0; i != child.Child.AlternativeRoots.Count; i++)
                    {
                        if (child.Child.AlternativeRoots[i].Equals(Annexed))
                            child.Child.AlternativeRoots[i] = Incorporating;
                    }

                Incorporating.Children.Add(child);
            }

            Incorporating.Content.Incorporate(ref Annexed.Content);
            Annexed = Incorporating;
        }

        internal static void Merge(ref Crisscross Merge1, ref Crisscross Merge2)
        {
            if (Merge1.CumulativedTransitionCharge < Merge2.CumulativedTransitionCharge)
                MergeK(ref Merge1, ref Merge2);
            else
                MergeK(ref Merge2, ref Merge1);

            if (!Merge1.Equals(Merge2))
                throw new Exception();
        }

        public void CopyTo(Array array, int index)
        {
            ToArray().CopyTo(array, index);
        }
    }
}
