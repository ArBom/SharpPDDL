using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class EffectExecutionException : Exception
    {
        public EffectExecutionException(string message) : base(message) { }
    }

    internal class WholeActionExecutionLambda : ExpressionVisitor
    {
        ObjectPDDL ActualEffectPDDL;
        List<ParameterExpression> Parameters;
        List<ParameterExpression> _parameters = new List<ParameterExpression>();

        internal WholeActionExecutionLambda(IReadOnlyList<Parametr> Parameters, IReadOnlyList<PreconditionPDDL> ExecutionPrecondition, IReadOnlyList<EffectPDDL> ExecutionEffects, IReadOnlyList<Execution> executions )
        {
            IReadOnlyList<Execution> OldDataExecutions = executions.Where(e => e.WorkWithNewValues == false).ToList();
            IReadOnlyList<Execution> NewDataExecutions = executions.Where(e => e.WorkWithNewValues == true).ToList();

            for (int i = 0; i != Parameters.Count; i++)
            {
                ParameterExpression Param = Expression.Parameter(Parameters[i].Type, ExtensionMethods.LamdbaParamPrefix + i);
                _parameters.Add(Param);
            }

            List<ParameterExpression> ParametersVar = new List<ParameterExpression>();

            int UsingVar = ExecutionEffects.Count;
            int ExecutionEffectsArraySize = 2 * UsingVar + executions.Count;

            if (ExecutionEffectsArraySize == 0)
                return;

            Expression[] ExecutionEffectsArray = new Expression[ExecutionEffectsArraySize];
            for (int i = 0; i != UsingVar; i++)
            {
                //Create internal parameters
                int AllParamsDestPos = ExecutionEffects[i].AllParamsOfAct1ClassPos.Value;
                string DestinationMemberName = ExecutionEffects[i].DestinationMemberName;
                Type DestinationMemberType = Parameters[AllParamsDestPos].values.First(v => v.Name == DestinationMemberName).Type;
                ParameterExpression ParamVar = Expression.Variable(DestinationMemberType, _parameters[AllParamsDestPos].Name + "_" + DestinationMemberName);
                ParametersVar.Add(ParamVar);

                //Modify source lambda
                ActualEffectPDDL = ExecutionEffects[i];
                Expression modified = Visit(ExecutionEffects[i].SourceFunc);
                Expression ConvertExpression = Expression.Convert(modified, DestinationMemberType);
                ExecutionEffectsArray[i] = Expression.Assign(ParamVar, ConvertExpression);

                //Update values
                MemberInfo DestinationMemberInfo = Parameters[AllParamsDestPos].Type.GetMember(DestinationMemberName).First(m => (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property));
                MemberExpression DestinationMemberAccess = Expression.MakeMemberAccess(_parameters[AllParamsDestPos], DestinationMemberInfo);
                ExecutionEffectsArray[UsingVar + i + OldDataExecutions.Count] = Expression.Assign(DestinationMemberAccess, ParamVar);
            }

            for (int i = 0; i != OldDataExecutions.Count; i++)
            {
                //ExecutionEffectsArray[UsingVar + i] = Visit(OldDataExecutions[i].);
            }

            for (int i = 0; i != NewDataExecutions.Count; i++)
            {
                //ExecutionEffectsArray[2*UsingVar + OldDataExecutions.Count + i ] = Visit(NewDataExecutions[i].);
            }

            BlockExpression ExecutingExpression = Expression.Block(
               ParametersVar,
               ExecutionEffectsArray
            );

            LambdaExpression lambdaExpression;

            if (ExecutionPrecondition.Count != 0)
            {
                ActualEffectPDDL = ExecutionPrecondition[0];
                Expression Checking = Expression.IfThenElse(Visit(ExecutionPrecondition[0].func), ExecutingExpression, Expression.Throw(Expression.Constant(new EffectExecutionException(ExecutionPrecondition[0].Name))));
                if (ExecutionPrecondition.Count > 1)
                {
                    for (int i = 1; i != ExecutionPrecondition.Count; i++)
                    {
                        ActualEffectPDDL = ExecutionPrecondition[i];
                        Checking = Expression.IfThenElse(Visit(ExecutionPrecondition[i].func), Checking, Expression.Throw(Expression.Constant(new EffectExecutionException(ExecutionPrecondition[i].Name))));
                    }
                }

                lambdaExpression = Expression.Lambda(Checking, _parameters);
            }
            else
                lambdaExpression = Expression.Lambda(ExecutingExpression, _parameters);

            var y = lambdaExpression.Compile();
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression NodeL = node.Body;
            if (node.Body.NodeType == ExpressionType.Convert)
                NodeL = ((UnaryExpression)node.Body).Operand;
            else
                NodeL = node.Body;

            Parameters = node.Parameters.ToList();

            return Visit(NodeL);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example string("Parameter") 
            string memberExpressionName = node.Name;

            int index = Parameters.FindIndex(p => p.Name == node.Name);

            if (ActualEffectPDDL is EffectPDDL)
            {
                int ParamCount = ((LambdaExpression)((EffectPDDL)ActualEffectPDDL).SourceFunc).Parameters.Count();

                //Tag: index
                if (ParamCount == 1)
                    index++;
            }

            switch (index)
            {
                case 0:
                    return _parameters[ActualEffectPDDL.AllParamsOfAct1ClassPos.Value];
                case 1:
                    return _parameters[ActualEffectPDDL.AllParamsOfAct2ClassPos.Value];  
            }

            throw new Exception();
        }
    }
}
