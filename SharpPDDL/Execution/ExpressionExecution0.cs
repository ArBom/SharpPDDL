using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class ExpressionExecution : ObjectPDDL
    {
        readonly internal bool WorkWithNewValues;

        internal protected Expression _Func;
        internal protected virtual Expression Func
        {
            get { return _Func; }
            protected set
            {
                if (_Func is null)
                    _Func = value;
            }
        }

        internal ExpressionExecution(string Name, Expression Func, bool WorkWithNewValues, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null)
            : base (Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class)
        {
            this.Func = Func;
            this.WorkWithNewValues = WorkWithNewValues;
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters) { }
    }
}
