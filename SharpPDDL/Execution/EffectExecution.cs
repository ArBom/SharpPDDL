using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SharpPDDL
{
    internal class EffectExecution : Execution
    {
        readonly EffectPDDL SourceEffectPDDL;

        internal EffectExecution(EffectPDDL SourceEffectPDDL) : base(SourceEffectPDDL.Name)
        {
            this.SourceEffectPDDL = SourceEffectPDDL;
        }

        internal override Delegate CreateEffectDelegate(IReadOnlyList<Parametr> Parameters)
        {
            EffectLambdaExecution effectLambdaExecution = new EffectLambdaExecution(SourceEffectPDDL);
            effectLambdaExecution.Visit(SourceEffectPDDL.SourceFunc);
            return effectLambdaExecution.EffectDel;
        }
    }

    internal class EffectLambdaExecution : ExpressionVisitor
    {
        internal Delegate EffectDel = null;
        readonly MemberInfo Dest;
        readonly string Name;
        private ReadOnlyCollection<ParameterExpression> _parameters;
        private ReadOnlyCollection<ParameterExpression> OldParameters;

        //konstruktor
        internal EffectLambdaExecution (EffectPDDL SourceEffectPDDL)
        {
            //MemberInfo of destination
            Dest = SourceEffectPDDL.TypeOf1Class.GetMember(SourceEffectPDDL.DestinationMemberName).First(m => m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field);
            Name = SourceEffectPDDL.Name;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            //Parameters
            OldParameters = node.Parameters;
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");
            
            //Make access to destination
            Expression DestAcc = Expression.MakeMemberAccess(_parameters[0], Dest);

            //set new _parameters somewhere inside this
            Expression ChanBody = Visit(node.Body);

            //Make expression to assign body with new params to the destination
            Expression Assign = Expression.Assign(DestAcc, ChanBody);

            //Marge it all
            LambdaExpression ModifiedFunct = Expression.Lambda(Assign, Name, _parameters);

            try
            {
                EffectDel = ModifiedFunct.Compile();
            }
            catch
            {
                throw new Exception("New func cannot be compilated.");
            }

            return ModifiedFunct;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameters.First(p => p.Name == node.Name);
        }
    }
}
