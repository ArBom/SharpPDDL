using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpPDDL
{
    internal class Crisscross : ICollection
    {
        public PossibleState Content;
        public Crisscross Root;
        public List<Crisscross> AlternativeRoots;
        public List<CrisscrossChildrenCon> Children;
        public UInt32 CumulativedTransitionCharge { get; private set; }
        private static EqComp eqComp;

        internal Crisscross()
        {
            this.Root = null;
            this.AlternativeRoots = new List<Crisscross>();
            this.Children = new List<CrisscrossChildrenCon>();
            this.Content = null;
            this.CumulativedTransitionCharge = 0;

            if (eqComp is null)
                eqComp = new EqComp();
        }

        public void Add(PossibleState item) => this.Add(item, 0, new object[0], 1, out Crisscross C);

        public void Add(PossibleState item, int ActionNr, object[] ActionArg, UInt32 AddedTransitionCharge, out Crisscross AddedItem)
        {
            AddedItem = new Crisscross()
            {
                Root = this,
                Content = item,
                CumulativedTransitionCharge = this.CumulativedTransitionCharge + AddedTransitionCharge
            };

            lock(Children)
                this.Children.Add(new CrisscrossChildrenCon(AddedItem, ActionNr, ActionArg, AddedTransitionCharge));
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
        
        // Nested class to do descending sort on make property.
        private class SortCumulativedTransitionChargeHelper : IComparer<Crisscross>
        {
            public int Compare(Crisscross c1, Crisscross c2)
            {
                int CumulativedTransitionChargeRes = c1.CumulativedTransitionCharge.CompareTo(c2.CumulativedTransitionCharge);

                // In case of the same CumulativedTransitionCharges' value compare CheckSums
                if (CumulativedTransitionChargeRes == 0)
                    return c1.Content.CheckSum.CompareTo(c2.Content.CheckSum);

                return CumulativedTransitionChargeRes;
            }
        }

        // Method to return IComparer object for sort helper for SortedSet<> collection.
        internal static IComparer<Crisscross> SortCumulativedTransitionCharge() =>
             new SortCumulativedTransitionChargeHelper();

        private class EqComp : IEqualityComparer<Crisscross>
        {
            public bool Equals(Crisscross x, Crisscross y)
            {
                if ((x.Content is null) || (y.Content is null))
                    return false;

                if (x.Content.CheckSum != y.Content.CheckSum)
                    return false;

                for (ushort ListCounter = 0; ListCounter != x.Content.ThumbnailObjects.Count; ++ListCounter)
                {
                    if (x.Content.ThumbnailObjects[ListCounter].CheckSum == (y.Content.ThumbnailObjects[ListCounter].CheckSum))
                        continue;
                    else
                        return false;
                }

                return true;
            }

            public int GetHashCode(Crisscross obj)
            {
                return obj.Content.CheckSum.GetHashCode();
            }
        }

        internal static IEqualityComparer<Crisscross> IContentEqualityComparer =>
            new EqComp();

        public bool IsReadOnly => false;

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        private List<CrisscrossChildrenCon> Position(List<CrisscrossChildrenCon> previesly)
        {
            if (this.Root == null)
                return previesly;

            CrisscrossChildrenCon thisOfRoot = this.Root.Children.First(c => eqComp.Equals(c.Child, this));
            List<CrisscrossChildrenCon> current = new List<CrisscrossChildrenCon>();
            current.Add(thisOfRoot);
            current.AddRange(previesly);
            return this.Root.Position(current);
        }

        public List<CrisscrossChildrenCon> Position()
        {
            if (this.Root == null)
                return null;

            var thisOfRoot = this.Root.Children.First(c => eqComp.Equals(c.Child, this));
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
            bool CheckChildRoor(CrisscrossChildrenCon child, Crisscross AnnexedA)
            {
                if (child.Child.Root is null)
                    return false;

                return child.Child.Root.Equals(AnnexedA);
            }

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
                        CrisscrossChildrenCon Updated = new CrisscrossChildrenCon(Annexed.AlternativeRoots[AnnAltRootI].Children[i], Incorporating);
                        Annexed.AlternativeRoots[AnnAltRootI].Children[i] = Updated;
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
            }

            lock(Annexed.Children)
            foreach (var child in Annexed.Children)
            {
                if (CheckChildRoor(child, Annexed))
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
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 58, GloCla.ResMan.GetString("C9"));
                throw new Exception(GloCla.ResMan.GetString("C9"));
            }
        }

        public void CopyTo(Array array, int index)
        {
            ToArray().CopyTo(array, index);
        }
    }
}
