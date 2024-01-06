using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1, T2> : PreconditionPDDL<T1> where T1 : class where T2 : class
    {
        internal T2 t2;
        readonly Expression<Predicate<T1, T2>> func;

        private int T2Index(List<Parametr> listOfParams)
        {
            if (!TypeOf2Class.IsClass)
                return -2;

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash2Class)
                    continue;

                if (ReferenceEquals(listOfParams[index], t2))
                    return index;
            }
            return -1;
        }

        new protected (Func<ThumbnailObject, ThumbnailObject, bool>, Func<dynamic, dynamic, bool>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            return (CheckPDDP, Check, T1Index(listOfParams), T2Index(listOfParams));
        }

        internal PreconditionPDDL(string Name, ref T1 obj1, ref T2 obj2, Expression<Predicate<T1, T2>> func) : base(Name, ref obj1, obj2.GetType(), obj2.GetHashCode())
        {
            this.func = func;

            PreconditionLambdaModif preconditionLambdaModif = new PreconditionLambdaModif();
            preconditionLambdaModif.Visit(func);
            this.CheckPDDP = preconditionLambdaModif.ModifiedFunct;
            this.usedMembers1Class = preconditionLambdaModif.used[0];
            this.usedMembers2Class = preconditionLambdaModif.used[1];
            this.t1 = obj1;
            this.t2 = obj2;
        }
    }
}
