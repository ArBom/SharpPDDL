using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SharpPDDL
{
    internal class Crisscross : ICollection, IDisposable
    {
        internal PossibleState Content;
        internal Crisscross Root { get; private set; }
        internal List<Crisscross> AlternativeRoots = new List<Crisscross>();
        internal List<CrisscrossChildrenCon> Children = new List<CrisscrossChildrenCon>();
        internal readonly UInt32 CumulativedTransitionCharge;
        internal static EqComp eqComp;

        /// <summary>
        /// The root of new collection
        /// </summary>
        internal Crisscross(PossibleState Content)
        {
            if (eqComp is null)
                eqComp = new EqComp();

            this.Root = null;
            this.Content = Content;
            this.CumulativedTransitionCharge = 0;
        }

        /// <summary>
        /// collection continuation
        /// </summary>
        private Crisscross(Crisscross Root, PossibleState Content, uint AddedTransitionCharge)
        {
            this.Root = Root;
            this.Content = Content;
            this.CumulativedTransitionCharge = Root.CumulativedTransitionCharge + AddedTransitionCharge;
        }

        public void Add(PossibleState item) => this.Add(item, 0, new ThumbnailObject[0], 1, out Crisscross C);

        public void Add(PossibleState item, int ActionNr, ICollection<ThumbnailObject> ActionArg, UInt32 AddedTransitionCharge, out Crisscross AddedItem)
        {
            AddedItem = new Crisscross(this, item, AddedTransitionCharge);

            ThumbnailObject[] ActionArgT = ActionArg.ToArray();

            lock (Children)
                this.Children.Add(new CrisscrossChildrenCon(AddedItem, ActionNr, ActionArgT, AddedTransitionCharge));
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

        public IEnumerator GetEnumerator()
        {
            return new CrisscrossEnum(this);
        }

        // Method to return IComparer object for sort helper for SortedSet<> collection.
        internal static IComparer<Crisscross> SortCumulativedTransitionCharge() =>
             new SortCumulativedTransitionChargeHelper();

        internal static IEqualityComparer<Crisscross> IContentEqualityComparer =>
            new EqComp();

        public bool IsReadOnly => false;

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        private void Position(List<CrisscrossChildrenCon> previesly)
        {
            if (this.Root == null)
                return;

            CrisscrossChildrenCon thisOfRoot = this.Root.Children.Where(c => eqComp.Equals(c.Child, this)).OrderBy(c => c.ActionCost).First();
            previesly.Add(thisOfRoot);
            this.Root.Position(previesly);
        }

        internal List<CrisscrossChildrenCon> Position()
        {
            List<CrisscrossChildrenCon> ToRet = new List<CrisscrossChildrenCon>();

            if (this.Root is null)
                return ToRet;

            Position(ToRet);
            ToRet.Reverse();

            return ToRet;
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

        private static void MergeK_AltRoots(Crisscross Incorporating, Crisscross Annexed)
        {
            //for every Alternative root of Annexed...
            foreach (Crisscross Annexex_AltRoot in Annexed.AlternativeRoots)
            {
                //Zmiana childrena
                for (int i = 0; i != Annexex_AltRoot.Children.Count; i++)
                {
                    try
                    {
                        if (Annexex_AltRoot[i].Equals(Annexed))
                        {
                            CrisscrossChildrenCon Updated = new CrisscrossChildrenCon(Annexex_AltRoot.Children[i], Incorporating);
                            Annexex_AltRoot.Children[i] = Updated;
                            break;
                        }
                    }
                    catch { /*todo;*/ }
                }

                //...add it to Incorporated AlternativeRoots
                bool IncorporatingAltRoorInclude = false;
                for (int IncoAltRootI = 0; IncoAltRootI != Incorporating.AlternativeRoots.Count; IncoAltRootI++)
                {
                    if (Incorporating.AlternativeRoots[IncoAltRootI].Equals(Annexex_AltRoot))
                    {
                        IncorporatingAltRoorInclude = true;
                        break;
                    }
                }

                if (!IncorporatingAltRoorInclude)
                {
                    if (Incorporating.Root is null)
                        Incorporating.AlternativeRoots.Add(Annexex_AltRoot);
                    else if (!Incorporating.Root.Equals(Annexex_AltRoot))
                        Incorporating.AlternativeRoots.Add(Annexex_AltRoot);
                }
            }
        }

        private static void MergeK_Child(Crisscross Incorporating, Crisscross Annexed)
        {
            lock (Annexed.Children)
            {
                foreach (var child in Annexed.Children)
                {
                    if (child.CheckChildMainRoot(Annexed))
                        child.Child.Root = Incorporating;
                    else
                        for (int i = 0; i != child.Child.AlternativeRoots.Count; i++)
                        {
                            if (child.Child.AlternativeRoots[i].Equals(Annexed))
                                child.Child.AlternativeRoots[i] = Incorporating;
                        }

                    lock (Incorporating.Children)
                        Incorporating.Children.Add(child);
                }

                Annexed.Children = Incorporating.Children;
            }
        }

        internal static void MergeK(Crisscross Incorporating, ref Crisscross Annexed)
        {
            //move Incorporating root to Annexed Alt. roots
            if (!(Incorporating.Root is null) && !(Annexed.Root is null))
                if (!Incorporating.Root.Equals(Annexed.Root))
                    Annexed.AlternativeRoots.Add(Annexed.Root);

            //work with roots
            MergeK_AltRoots(Incorporating, Annexed);

            //work with child
            MergeK_Child(Incorporating, Annexed);

            //trade up to
            Incorporating.Content.Incorporate(ref Annexed.Content);
            Annexed.Content = Incorporating.Content;
            Annexed.Root = Incorporating.Root;
            Annexed = Incorporating;
        }

        internal static void Merge(ref Crisscross Merge1, ref Crisscross Merge2)
        {
            if (Merge1.CumulativedTransitionCharge < Merge2.CumulativedTransitionCharge)
                MergeK(Merge1, ref Merge2);
            else
                MergeK(Merge2, ref Merge1);

            if (!Object.ReferenceEquals(Merge1,Merge2))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 58, GloCla.ResMan.GetString("C9"));
                throw new Exception(GloCla.ResMan.GetString("C9"));
            }
        }

        public void CopyTo(Array array, int index)
        {
            ToArray().CopyTo(array, index);
        }

        public void Dispose()
        {
            if (AlternativeRoots is null)
                return;

            AlternativeRoots.Clear();
            AlternativeRoots = null;

            if (Children?.Count != 0)
            {
                for (int i = this.Children.Count - 1; i != 0; i--)
                    this[i].Dispose();

                Children = null;
            }

            Content?.Dispose();
            Content = null;
        }
    }
}
