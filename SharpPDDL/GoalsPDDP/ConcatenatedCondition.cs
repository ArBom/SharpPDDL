using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal class ConcatenatedCondition
    {
        LambdaExpression expression;
        readonly int Thumbnail1;
        readonly int Thumbnail2;

        internal ConcatenatedCondition(LambdaExpression expression, int Thumbnail1, int Thumbnail2)
        {
            this.expression = expression;
            this.Thumbnail1 = Thumbnail1;
            this.Thumbnail2 = Thumbnail2;
        }
    }
}
