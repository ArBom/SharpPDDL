﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    class CrisscrossEnum<T> : IEnumerator<T> where T : class
    {
        private Crisscross<T> current;
        private readonly Crisscross<T> creator;
        private List<KeyValuePair<Crisscross<T>, Crisscross<T>>> UsedAlternativeRoots;

        internal CrisscrossEnum(Crisscross<T> creator)
        {
            this.creator = creator;
            this.UsedAlternativeRoots = new List<KeyValuePair<Crisscross<T>, Crisscross<T>>>();
        }

        T IEnumerator<T>.Current => current.Content;

        object IEnumerator.Current => throw new NotImplementedException();

        void IDisposable.Dispose() { }

        protected bool MoveNextFromLine(Crisscross<T> e)
        {
            if (e.Root is null)
                return false;

            Crisscross<T> TempRoot = null;

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

            int IndeksOfE = TempRoot.Children.IndexOf(e);

            if (TempRoot.Children.Count != ++IndeksOfE)
            {
                e = TempRoot.Children[IndeksOfE];
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
                if (current.Children[0].Root != current)
                {
                    var AlternativeRootEntrance = new KeyValuePair<Crisscross<T>, Crisscross<T>>(current.Children[0], current);
                    UsedAlternativeRoots.Insert(0, AlternativeRootEntrance);
                }

                current = current.Children[0];
                return true;
            }

            return MoveNextFromLine(current);
        }

        void IEnumerator.Reset()
        {
            Crisscross<T> MinusOnePos = new Crisscross<T>
            {
                Children = new List<Crisscross<T>> { creator }
            };
            this.current = MinusOnePos;
            this.UsedAlternativeRoots = new List<KeyValuePair<Crisscross<T>, Crisscross<T>>>();
        }
    }
}
