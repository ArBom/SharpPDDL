using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal class ConcatenatedCondition
    {
        LambdaExpression expression;

        internal ConcatenatedCondition(LambdaExpression expression)
        {
            this.expression = expression;
        }
    }
}
