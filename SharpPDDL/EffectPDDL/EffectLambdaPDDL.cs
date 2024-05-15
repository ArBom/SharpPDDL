using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{ 
    internal class EffectLambdaPDDL : ExpressionVisitor
    {

        private ReadOnlyCollection<ParameterExpression> _parameters;
        private ReadOnlyCollection<ParameterExpression> OldParameters;
        public Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>> ModifiedFunct;
        private readonly int[] ParamsIndexesInAction;
        readonly List<SingleTypeOfDomein> allTypes;
        readonly Expression FuncOutKey = null;

        public EffectLambdaPDDL(List<SingleTypeOfDomein> allTypes, int[] paramsIndexesInAction, ushort funcOutKey)
        {
            if (allTypes is null)
                throw new Exception();

            if (allTypes.Count == 0)
                throw new Exception();

            if (paramsIndexesInAction is null)
                throw new Exception();

            if (paramsIndexesInAction.Length == 0)
                throw new Exception();

            this.allTypes = allTypes;
            this.ParamsIndexesInAction = paramsIndexesInAction;
            this.FuncOutKey = Expression.Constant(funcOutKey, typeof(ushort));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            OldParameters = node.Parameters;
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");

            if (OldParameters.Count() == 0)
            {
                Collection<ParameterExpression> parameterExpressions = new Collection<ParameterExpression>();
                parameterExpressions.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix + ParamsIndexesInAction[0]));
                parameterExpressions.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), "empty"));
                _parameters = new ReadOnlyCollection<ParameterExpression>(parameterExpressions);
            }

            //the library use only 1- or 2-Parameter lambdas
            if (OldParameters.Count() > 2)
            {
                throw new Exception();
            }

            //make 2-Parameters lambda
            if (OldParameters.Count() == 1)
            {
                Collection<ParameterExpression> parameterExpressions = new Collection<ParameterExpression>();
                parameterExpressions.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix + ParamsIndexesInAction[0]));
                parameterExpressions.Add(Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix + ParamsIndexesInAction[1]));
                _parameters = new ReadOnlyCollection<ParameterExpression>(parameterExpressions);
            }

            var ResultType = typeof(KeyValuePair<ushort, ValueType>).GetConstructors()[0];
            Expression[] param = { FuncOutKey, Visit(node.Body) };
            NewExpression expectedTypeExpression = Expression.New(ResultType, param);
            ModifiedFunct = Expression.Lambda<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, KeyValuePair<ushort, ValueType>>>(expectedTypeExpression, _parameters);

            try
            {
                _ = ModifiedFunct.Compile();
            }
            catch
            {
                throw new Exception("New func cannot be compilated.");
            }

            return ModifiedFunct;
        }

        private string NewParamName(string OldNodeName)
        {
            var param = OldParameters.First(p => p.Name == OldNodeName);
            int index = OldParameters.IndexOf(param)+1;
            return ExtensionMethods.LamdbaParamPrefix + ParamsIndexesInAction[index];
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            string NewParamname = NewParamName(node.Name);
            return Expression.Parameter(typeof(PossibleStateThumbnailObject), NewParamname);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            return node.Update(left, VisitAndConvert(node.Conversion, "VisitBinary"), right);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant)
                return node;

            //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example string("Parameter") 
            string memberExpressionName = node.Expression.ToString();

            string newParamName = NewParamName(memberExpressionName);
            ParameterExpression newParam = _parameters.First(p => p.Name == newParamName);

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            //intersect
            SingleTypeOfDomein ParameterModel = allTypes.Where(t => t.Type == node.Expression.Type)?.First();

            if (ParameterModel is null)
            {
                throw new Exception();
            }

            ///TODO in next versipn
            /*var Inter = l.Select(el => (el, el.Item2.Interfaces.Count()))?.Where(k => k.Item2 != 0)?.OrderBy(k => k.Item2).Select(k => k.el).ToList();
            for (int a=0; a!=Inter.Count(); ++a)
            {
                Inter[a].p.Value
            }*/
            ///in next version

            ushort ValueOfIndexesKey = ParameterModel.CumulativeValues.Where(v => v.Name == MemberName).Select(v => v.ValueOfIndexesKey).First();

            Expression[] argument = new[] { Expression.Constant(ValueOfIndexesKey) };

            //Property of ThumbnailObject.this[uint key]
            PropertyInfo TO_indekser = typeof(PossibleStateThumbnailObject).GetProperty("Item");

            //Make expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
            IndexExpression IndexAccessExpr = Expression.MakeIndex(newParam, TO_indekser, argument);

            //Convert above expression from ValueType to particular type of frontal value
            return Expression.Convert(IndexAccessExpr, node.Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new Exception("You cannot to use object method call to create model of object. Try to write this method (" + node.ToString() + ")as new lambda which uses only ValueType member(s) of object.");
        }
    }
}