using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    class CrisscrossEnum : IEnumerator
    {
        private Crisscross current;
        private readonly Crisscross creator;
        private List<KeyValuePair<Crisscross, Crisscross>> UsedAlternativeRoots;

        internal CrisscrossEnum(Crisscross creator)
        {
            this.creator = creator;
            this.UsedAlternativeRoots = new List<KeyValuePair<Crisscross, Crisscross>>();
        }

        Crisscross Current => current;

        object IEnumerator.Current => throw new NotImplementedException();

        protected bool MoveNextFromLine(Crisscross e)
        {
            if (e.Root is null)
                return false;

            Crisscross TempRoot = null;

            if (UsedAlternativeRoots.Count != 0)
            {
                if (UsedAlternativeRoots[0].Key == e)
                {
                    TempRoot = UsedAlternativeRoots[0].Value;
                    UsedAlternativeRoots.RemoveAt(0);
                }
            }

            if (TempRoot is null)
                TempRoot = e.Root;

            int IndeksOfE = TempRoot.Children.FindIndex(c => c.Child == e);

            if (TempRoot.Children.Count != ++IndeksOfE)
            {
                e = TempRoot.Children[IndeksOfE].Child;
                return true;
            }
            else
            {
                return MoveNextFromLine(TempRoot);
            }
        }

        public bool MoveNext()
        {
            if (current.Children.Count != 0)
            {
                if (current.Children[0].Child.Root != current)
                {
                    var AlternativeRootEntrance = new KeyValuePair<Crisscross, Crisscross>(current.Children[0].Child, current);
                    UsedAlternativeRoots.Insert(0, AlternativeRootEntrance);
                }

                current = current.Children[0].Child;
                return true;
            }

            return MoveNextFromLine(current);
        }

        void IEnumerator.Reset()
        {
            Crisscross MinusOnePos = new Crisscross
            {
                Children = new List<CrisscrossChildrenCon> {new CrisscrossChildrenCon(creator, 0, null)}
            };
            this.current = MinusOnePos;
            this.UsedAlternativeRoots = new List<KeyValuePair<Crisscross, Crisscross>>();
        }
    }
}
