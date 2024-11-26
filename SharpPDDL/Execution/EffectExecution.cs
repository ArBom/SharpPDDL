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
        new Delegate Delegate;

        internal EffectExecution(EffectPDDL SourceEffectPDDL) : base(SourceEffectPDDL.Name)
        {
            this.SourceEffectPDDL = SourceEffectPDDL;
        }

        internal override Delegate CreateEffectDelegate(IReadOnlyList<Parametr> Parameters)
        {
            EffectLambdaExecution effectLambdaExecution = new EffectLambdaExecution(SourceEffectPDDL, Parameters);

            //effectLambdaExecution.Visit(SourceEffectPDDL.SourceFunc);

            this.Delegate = effectLambdaExecution.EffectDel;
            return effectLambdaExecution.EffectDel;
        }
    }

    internal class EffectLambdaExecution : ExpressionVisitor
    {
        internal Delegate EffectDel = null;
        readonly string Name;
        readonly EffectPDDL SourceEffectPDDL;

        private readonly int[] ParamsIndexesInAction;

        //new parameters with oryginal type with lenght and order like "Parameters" List bellow
        private readonly IReadOnlyList<ParameterExpression> _parameters;

        //old parameters of source lambda
        private List<ParameterExpression> OldParameters;

        //Parameters of whole Action
        IReadOnlyList<Parametr> Parameters;

        //konstruktor
        internal EffectLambdaExecution (EffectPDDL SourceEffectPDDL, IReadOnlyList<Parametr> Parameters)
        {
            Name = SourceEffectPDDL.Name;
            this.SourceEffectPDDL = SourceEffectPDDL;
            this.Parameters = Parameters;

            ParamsIndexesInAction = new int[] { SourceEffectPDDL.AllParamsOfAct1ClassPos.Value };
            if (SourceEffectPDDL.AllParamsOfAct2ClassPos.HasValue)
                ParamsIndexesInAction = new int[] { SourceEffectPDDL.AllParamsOfAct1ClassPos.Value, SourceEffectPDDL.AllParamsOfAct2ClassPos.Value };

            //Create ParameterExpressions from Parameters
            List<ParameterExpression> parameters = new List<ParameterExpression>();
            for (int i = 0; i!= Parameters.Count; i++)
            {
                ParameterExpression parameterExpression = Expression.Parameter(Parameters[i].Type, ExtensionMethods.LamdbaParamPrefix + i);
                parameters.Add(parameterExpression);
            }
            _parameters = new List<ParameterExpression>(parameters);

            //MemberInfo of destination
            MemberInfo Dest = SourceEffectPDDL.TypeOf1Class.GetMember(SourceEffectPDDL.DestinationMemberName).First(m => (m.MemberType == MemberTypes.Property || m.MemberType == MemberTypes.Field));

            //Make an access to destination
            Expression DestAcc = Expression.MakeMemberAccess(_parameters[SourceEffectPDDL.AllParamsOfAct1ClassPos.Value], Dest);

            Expression PrecoLambdaBody;
            if (SourceEffectPDDL.SourceFunc.NodeType == ExpressionType.Lambda)
            {
                LambdaExpression TempLambda = (LambdaExpression)SourceEffectPDDL.SourceFunc;
                this.OldParameters = new List<ParameterExpression>(TempLambda.Parameters);

                PrecoLambdaBody = Expression.Convert(Visit(TempLambda.Body), DestAcc.Type);
            }
            else if (SourceEffectPDDL.SourceFunc.NodeType == ExpressionType.Constant)
            {
                PrecoLambdaBody = Expression.Convert(SourceEffectPDDL.SourceFunc, DestAcc.Type);
            }
            else
                throw new Exception();

            //Make expression to assign body with new params to the destination
            Expression Assign = Expression.Assign(DestAcc, PrecoLambdaBody);

            //Marge it all
            LambdaExpression ModifiedFunct = Expression.Lambda(Assign, Name, _parameters);

            try
            {
                ModifiedFunct.Compile();
            }
            catch
            {
                throw new Exception();
            }
        }

        private string NewParamName(string OldNodeName)
        {
            ParameterExpression param = OldParameters.First(p => p.Name == OldNodeName);
            int index = OldParameters.IndexOf(param);

            if (OldParameters.Count == 1)
                index++;

            return ExtensionMethods.LamdbaParamPrefix + ParamsIndexesInAction[index];
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example string("Parameter")
                    string OldParamName = node.Expression.ToString();

                    string memberExpressionName = NewParamName(OldParamName);
                    MemberInfo memberInfo = node.Member;
                    ParameterExpression parameterExpression = _parameters.First(p => p.Name == memberExpressionName);
                    Expression expression = Expression.MakeMemberAccess(parameterExpression, memberInfo);               
                    return expression;

                case ExpressionType.Constant:
                    return node;

                default:
                    return node;
            }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameters.First(p => p.Name == node.Name);
        }
    }
}
