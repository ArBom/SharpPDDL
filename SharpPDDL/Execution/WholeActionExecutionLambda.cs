using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class WholeActionExecutionLambda : ExpressionVisitor
    {
        private ObjectPDDL ActualObjectPDDL;
        private List<ParameterExpression> LambdaParameters;
        private List<ParameterExpression> _parameters = new List<ParameterExpression>();

        private List<Expression> ExecutionEffectsArray1 = new List<Expression>();
        private List<Expression> ExecutionEffectsArray2 = new List<Expression>();
        private List<Expression> ExecutionEffectsArray3 = new List<Expression>();
        private List<Expression> ExecutionEffectsArray4 = new List<Expression>();

        internal readonly Delegate InstantExecutionPDDL;

        internal WholeActionExecutionLambda(string Name, IReadOnlyList<Parametr> Parameters, IReadOnlyList<PreconditionPDDL> ExecutionPrecondition, IReadOnlyList<EffectPDDL> ExecutionEffects, IReadOnlyList<ExpressionExecution> executions)
        {
            foreach (ExpressionExecution execution in executions)
                execution.CompleteClassPos(Parameters);

            for (int i = 0; i != Parameters.Count; i++)
            {
                ParameterExpression Param = Expression.Parameter(Parameters[i].Type, GloCla.LamdbaParamPrefix + i);
                _parameters.Add(Param);
            }

            IEnumerable<ParameterExpression> ParametersVar = ModifyExecutionEffects(Parameters, ExecutionEffects);

            foreach (ExpressionExecution Execution in executions)
            {
                ActualObjectPDDL = Execution;
                Expression toadd = Visit(Execution.Func);

                if (Execution.WorkWithNewValues)
                    ExecutionEffectsArray4.Add(toadd);
                else
                    ExecutionEffectsArray2.Add(toadd);
            }

            LambdaExpression lambdaExpression = MergeItAll(ParametersVar, ExecutionPrecondition, Name);

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

        protected LambdaExpression MergeItAll(IEnumerable<ParameterExpression> ParametersVar, IReadOnlyList<PreconditionPDDL> ExecutionPrecondition, string Name)
        {
            BlockExpression ExecutingExpression = Expression.Block(
                ParametersVar,
                ExecutionEffectsArray1.Concat(ExecutionEffectsArray2).Concat(ExecutionEffectsArray3).Concat(ExecutionEffectsArray4)
            );

            Expression IfTrue = ExecutingExpression;
            foreach (PreconditionPDDL preconditionPDDL in ExecutionPrecondition)
            {
                ActualObjectPDDL = preconditionPDDL;
                Expression Test = Visit(preconditionPDDL.func);
                UnaryExpression IfFalse = Expression.Throw(Expression.Constant(new PrecondExecutionException(Name, preconditionPDDL.Name)));
                IfTrue = Expression.IfThenElse(Test, IfTrue, IfFalse);
            }
            return Expression.Lambda(IfTrue, Name, _parameters);
        }

        protected void ModifyExEffSourceL(EffectPDDL effectPDDL, ParameterExpression ParamVar)
        {
            ActualObjectPDDL = effectPDDL;

            Expression ConvertExpression;

            if (effectPDDL is EffectPDDLpointer)
            {
                switch (effectPDDL.Elements.Count())
                {
                    case 1:
                        ConvertExpression = Expression.Constant(null, ParamVar.Type);
                        break;

                    case 2:
                        ConvertExpression = _parameters.First(p => p.Name == GloCla.LamdbaParamPrefix + effectPDDL.Elements[1].AllParamsOfActClassPos);
                        break;

                    default:
                        throw new Exception();
                }
            }
            else
            {
                Expression modifiedSourceFunc = Visit(effectPDDL.SourceFunc);
                ConvertExpression = Expression.Convert(modifiedSourceFunc, ParamVar.Type);
            }

            ExecutionEffectsArray1.Add(Expression.Assign(ParamVar, ConvertExpression));
        }

        protected IEnumerable<ParameterExpression> ModifyExecutionEffects(IReadOnlyList<Parametr> Parameters, IEnumerable<EffectPDDL> ExecutionEffects)
        {
            List<ParameterExpression> ParametersVar = new List<ParameterExpression>();

            foreach (EffectPDDL effectPDDL in ExecutionEffects)
            {
                //Create internal parameters
                int AllParamsDestPos = effectPDDL.Elements.First().AllParamsOfActClassPos.Value;
                string DestinationMemberName = effectPDDL.DestinationMemberName;
                Type DestinationMemberType = Parameters[AllParamsDestPos].values.First(v => v.Name == DestinationMemberName).Type;
                ParameterExpression ParamVar = Expression.Variable(DestinationMemberType, _parameters[AllParamsDestPos].Name + "_" + DestinationMemberName);
                ParametersVar.Add(ParamVar);

                //Modify source lambda
                ModifyExEffSourceL(effectPDDL, ParamVar);

                //Update values
                MemberInfo DestinationMemberInfo = Parameters[AllParamsDestPos].Type.GetMember(DestinationMemberName).First(m => (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property));
                MemberExpression DestinationMemberAccess = Expression.MakeMemberAccess(_parameters[AllParamsDestPos], DestinationMemberInfo);
                ExecutionEffectsArray3.Add(Expression.Assign(DestinationMemberAccess, ParamVar));
            }

            return ParametersVar;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression NodeL = node.Body;
            if (node.Body.NodeType == ExpressionType.Convert)
                NodeL = ((UnaryExpression)node.Body).Operand;
            else
                NodeL = node.Body;

            LambdaParameters = node.Parameters.ToList();

            return Visit(NodeL);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            string ExceptionMessage;

            try
            {
                int index = LambdaParameters.FindIndex(p => p.Name == node.Name);

                if (ActualObjectPDDL is EffectPDDL && ActualObjectPDDL.Elements.Length == 2)
                {
                    EffectPDDL ActualEffectPDDL = ActualObjectPDDL as EffectPDDL;
                    if (ActualEffectPDDL.SourceFunc is LambdaExpression)
                    {
                        LambdaExpression lambdaExpression = ActualEffectPDDL.SourceFunc as LambdaExpression;
                        if (lambdaExpression.Parameters.Count() == 1)
                            index++;
                    }
                }

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
