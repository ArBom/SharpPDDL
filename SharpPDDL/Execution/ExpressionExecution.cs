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

        internal ExpressionExecution(string Name, Expression Func, bool WorkWithNewValues, object[] ElementsInOnbjectPDDL)
            : base (Name, ElementsInOnbjectPDDL)
        {
            this.Func = Func;
            this.WorkWithNewValues = WorkWithNewValues;
        }

        override internal void CompleteActinParams(IList<Parametr> Parameters) => Expression.Empty();
    }
}
