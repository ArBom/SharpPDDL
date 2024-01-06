using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1> : PreconditionPDDL where T1 : class
    {
        protected T1 t1;
        readonly Expression<Predicate<T1>> func;

        protected int T1Index(List<Parametr> listOfParams)
        {
            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash1Class)
                    continue;

                if (ReferenceEquals(listOfParams[index], t1))
                    return index;
            }
            return -1;
        }

        internal override (Func<ThumbnailObject, ThumbnailObject, bool>, Func<dynamic, dynamic, bool>) TakeFunct()
        {
            return (CheckPDDP, Check);
        }

        protected PreconditionPDDL(string Name, ref T1 obj1, Type TypeOf2Class, Int32 HashOf2Class) : base(Name, obj1.GetType(), obj1.GetHashCode(), TypeOf2Class, HashOf2Class)
        {
            this.t1 = obj1;
        }

        internal PreconditionPDDL(string Name, ref T1 obj1, Expression<Predicate<T1>> func) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.func = func;

            PreconditionLambdaModif preconditionLambdaModif = new PreconditionLambdaModif();
            preconditionLambdaModif.Visit(func);
            this.CheckPDDP = preconditionLambdaModif.ModifiedFunct;
            this.usedMembers1Class = preconditionLambdaModif.used[0];
            this.t1 = obj1;
        }
    }
}