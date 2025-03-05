using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SharpPDDL
{
    class CrisscrossEnum : IEnumerator<Crisscross>
    {
        internal bool Repeated;
        protected Crisscross current;
        protected readonly Crisscross creator;
        internal List<ChainStruct> UsedAlternativeRoots { get; private set; }

        public Crisscross Current => current;
        object IEnumerator.Current => current;

        public CrisscrossChildrenCon CurrentConnector;

        internal CrisscrossEnum(Crisscross creator)
        {
            this.creator = creator;
            this.Reset();
        }

        protected bool MoveNextFromLine()
        {
            if (!UsedAlternativeRoots.Any())
                return false;

            Repeated = false;
            ChainStruct LastOnList = UsedAlternativeRoots.Last();
            
            if(LastOnList.Chain.Children.Count()-1 != LastOnList.ChainChildNo)
            {
                CurrentConnector = LastOnList.Chain.Children[++LastOnList.ChainChildNo];
                current = CurrentConnector.Child;
                Repeated = UsedAlternativeRoots.Any(t => t.Chain.Equals(current));
                return true;
            }

            UsedAlternativeRoots.Remove(LastOnList);
            return MoveNextFromLine();
        }

        public bool MoveNext()
        {
            if (!Repeated && current.Children.Any())
            {
                ChainStruct UsedAlternativeRootsToAdd = new ChainStruct(current, 0);
                Repeated = UsedAlternativeRoots.Any(t => t.Chain.Equals(current));
                UsedAlternativeRoots.Add(UsedAlternativeRootsToAdd);
                CurrentConnector = current.Children[0];
                current = CurrentConnector.Child;
                return true;
            }

            return MoveNextFromLine();
        }

        public void Reset()
        {
            Crisscross MinusOnePos = new Crisscross
            {
                Children = new List<CrisscrossChildrenCon> { new CrisscrossChildrenCon(creator, 0, null, 0) }
            };

            this.Repeated = false;
            this.current = MinusOnePos;
            this.UsedAlternativeRoots = new List<ChainStruct>();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
