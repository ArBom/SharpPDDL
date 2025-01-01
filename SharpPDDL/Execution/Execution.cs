using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal abstract class Execution : ObjectPDDL
    {
        readonly internal bool WorkWithNewValues;
        internal Delegate Delegate;

        internal Expression _Func;
        internal protected virtual Expression Func
        {
            get { return _Func; }
            protected set
            {
                if (_Func is null)
                    _Func = value;
            }
        }

        internal Execution(string Name, Expression Func, bool WorkWithNewValues, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null)
            : base (Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class)
        {
            this.Func = Func;
            this.WorkWithNewValues = WorkWithNewValues;
        }

        protected int? Index<T>(T t, IReadOnlyList<Parametr> Parameters)
        {
            for (int index = 0; index != Parameters.Count; index++)
            {
                if (Parameters[index].HashCode != t.GetHashCode())
                    continue;

                if (t.Equals(Parameters[index].Oryginal))
                {
                    return index;
                }
            }

            return null;
        }
    }
}
