using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class PrecondExecutionException : Exception
    {
        internal const string ActionName = "ActionName";
        internal const string ExecutionPreconditionName = "ExecutionPrecondition";

        public PrecondExecutionException(string ActionName, string ExecutionPreconditionName) : base("Unexpected value in time of trying realize " + ActionName + " action; unfulfil " + ExecutionPreconditionName + " precondition")
        {
            Data.Add(PrecondExecutionException.ActionName, ActionName);
            Data.Add(PrecondExecutionException.ExecutionPreconditionName, ExecutionPreconditionName);
        }
    }

    internal class WholeActionExecutionLambda : ExpressionVisitor
    {
        ObjectPDDL ActualObjectPDDL;
        List<ParameterExpression> Parameters;
        List<ParameterExpression> _parameters = new List<ParameterExpression>();

        internal readonly Delegate InstantExecutionPDDL;

        internal WholeActionExecutionLambda(string Name, IReadOnlyList<Parametr> Parameters, IReadOnlyList<PreconditionPDDL> ExecutionPrecondition, IReadOnlyList<EffectPDDL> ExecutionEffects, IReadOnlyList<ExpressionExecution> executions)
        {
            foreach (ExpressionExecution execution in executions)
                execution.CompleteClassPos(Parameters);

            IReadOnlyList<ExpressionExecution> OldDataExecutions = executions.Where(e => e.WorkWithNewValues == false).ToList();
            IReadOnlyList<ExpressionExecution> NewDataExecutions = executions.Where(e => e.WorkWithNewValues == true).ToList();

            for (int i = 0; i != Parameters.Count; i++)
            {
                ParameterExpression Param = Expression.Parameter(Parameters[i].Type, GloCla.LamdbaParamPrefix + i);
                _parameters.Add(Param);
            }

            List<ParameterExpression> ParametersVar = new List<ParameterExpression>();

            int UsingVar = ExecutionEffects.Count;
            int ExecutionEffectsArraySize = 2 * UsingVar + executions.Count;

            Expression[] ExecutionEffectsArray = new Expression[ExecutionEffectsArraySize];
            for (int i = 0; i != UsingVar; i++)
            {
                //Create internal parameters
                int AllParamsDestPos = ExecutionEffects[i].Elements[0].AllParamsOfActClassPos.Value;
                string DestinationMemberName = ExecutionEffects[i].DestinationMemberName;
                Type DestinationMemberType = Parameters[AllParamsDestPos].values.First(v => v.Name == DestinationMemberName).Type;
                ParameterExpression ParamVar = Expression.Variable(DestinationMemberType, _parameters[AllParamsDestPos].Name + "_" + DestinationMemberName);
                ParametersVar.Add(ParamVar);

                //Modify source lambda
                ActualObjectPDDL = ExecutionEffects[i];
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
                ActualObjectPDDL = OldDataExecutions[i];
                ExecutionEffectsArray[UsingVar + i] = Visit(OldDataExecutions[i].Func);
            }

            for (int i = 0; i != NewDataExecutions.Count; i++)
            {
                ActualObjectPDDL = OldDataExecutions[i];
                ExecutionEffectsArray[2*UsingVar + OldDataExecutions.Count + i ] = Visit(NewDataExecutions[i].Func);
            }

            BlockExpression ExecutingExpression = Expression.Block(
               ParametersVar,
               ExecutionEffectsArray
            );

            LambdaExpression lambdaExpression;

            if (ExecutionPrecondition.Any())
            {
                ActualObjectPDDL = ExecutionPrecondition[0];
                Expression Checking = Expression.IfThenElse(Visit(ExecutionPrecondition[0].func), ExecutingExpression, Expression.Throw(Expression.Constant(new PrecondExecutionException(Name, ExecutionPrecondition[0].Name))));
                if (ExecutionPrecondition.Count > 1)
                {
                    for (int i = 1; i != ExecutionPrecondition.Count; i++)
                    {
                        ActualObjectPDDL = ExecutionPrecondition[i];
                        Checking = Expression.IfThenElse(Visit(ExecutionPrecondition[i].func), Checking, Expression.Throw(Expression.Constant(new PrecondExecutionException(Name, ExecutionPrecondition[i].Name))));
                    }
                }

                lambdaExpression = Expression.Lambda(Checking, Name, _parameters);
            }
            else
                lambdaExpression = Expression.Lambda(ExecutingExpression, Name, _parameters);

            try
            {
                InstantExecutionPDDL = lambdaExpression.Compile();
            }
            catch
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C23"), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 86, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
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
            string ExceptionMessage;

            try
            {
                int index = Parameters.FindIndex(p => p.Name == node.Name);
                return _parameters[ActualObjectPDDL.Elements[index].AllParamsOfActClassPos.Value];
            }
            catch (ArgumentNullException)
            {
                ExceptionMessage = string.Format(GloCla.ResMan.GetString("C40"), node.Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 124, ExceptionMessage);
            }
            catch (IndexOutOfRangeException)
            {
                ExceptionMessage = GloCla.ResMan.GetString("C44");
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 146, ExceptionMessage);
            }
            catch (Exception e)
            {
                ExceptionMessage = e.Message;
            }

            throw new Exception(ExceptionMessage);
        }
    }
}
