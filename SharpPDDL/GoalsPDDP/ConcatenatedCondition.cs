using System;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class ConcatenatedCondition
    {
        LambdaExpression expression;
        internal readonly int Thumbnail1;
        internal readonly int Thumbnail2;
        internal readonly int BiggerThumbnail;

        private Delegate _GoalPDDL = null;
        internal Delegate GoalPDDL => _GoalPDDL;

        internal ConcatenatedCondition(LambdaExpression expression, int Thumbnail1, int Thumbnail2)
        {
            this.expression = expression;
            this.Thumbnail1 = Thumbnail1;
            this.Thumbnail2 = Thumbnail2;
            
            if(Thumbnail1 > Thumbnail2)
                this.BiggerThumbnail = Thumbnail1;
            else
                this.BiggerThumbnail = Thumbnail2;
        }

        internal LambdaExpression BuildGoalPDDP()
        {
            try
            {
                _GoalPDDL = expression.Compile();
            }
            catch (Exception e)
            {
                /*string ExceptionMess = String.Format(GloCla.ResMan.GetString("C29"), e.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 106, ExceptionMess);
                throw new Exception(ExceptionMess);*/
            }

            return expression;
        }
    }
}
