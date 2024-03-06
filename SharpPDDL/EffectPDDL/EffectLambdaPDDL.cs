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
        public Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>> ModifiedFunct;
        readonly List<SingleTypeOfDomein> allTypes;
        readonly Expression FuncOutKey = null;

        public EffectLambdaPDDL(List<SingleTypeOfDomein> allTypes, uint funcOutKey)
        {
            this.allTypes = allTypes;
            this.FuncOutKey = Expression.Constant(funcOutKey, typeof(uint));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (allTypes is null)
                throw new Exception();

            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");

            if (_parameters.Count() == 0)
            {
                //its no sens; its always true or always false
                throw new Exception();
            }

            //the library use only 1- or 2-Parameter lambdas
            if (_parameters.Count() > 2)
            {
                throw new Exception();
            }

            //make 2-Parameters lambda
            if (_parameters.Count() == 1)
            {
                string NameOfNewOne = _parameters.First().Name == "empty" ? "empty2" : "empty";
                List<ParameterExpression> parameters = _parameters.ToList<ParameterExpression>();
                parameters.Add(Expression.Parameter(typeof(ThumbnailObject), NameOfNewOne));
                _parameters = parameters.AsReadOnly();
            }

            Type ResultType = typeof(KeyValuePair<uint, ValueType>);
            NewExpression expectedTypeExpression = Expression.New(ResultType);
            PropertyInfo ValuePropertyInfo = ResultType.GetProperty("Value");
            PropertyInfo KeyPropertyInfo = ResultType.GetProperty("Key");
            MemberAssignment KeyAssignment = Expression.Bind(KeyPropertyInfo, FuncOutKey);
            MemberAssignment ValueAssignment = Expression.Bind(ValuePropertyInfo, Visit(node.Body));
            MemberInitExpression memberInit = Expression.MemberInit(expectedTypeExpression, KeyAssignment, ValueAssignment);

            var ModifeidLambda = Expression.Lambda<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>(memberInit, _parameters);
            ModifiedFunct = ModifeidLambda.Compile();
            return ModifeidLambda;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Expression.Parameter(typeof(ThumbnailObject), node.Name);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            return node.Update(left, VisitAndConvert(node.Conversion, "VisitBinary"), right);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            //its parameter from in front of arrow: Parameter => lambda(Parameter) ; in these example string("Parameter") 
            string memberExpressionName = node.Expression.ToString();

            List<Type> tg = node.Type.InheritedTypes().TypesAndInterfaces.ToList();

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            ushort ValuesDictKey = 0;
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

            ParameterExpression parameterExpression;

            //adding expression in use to the list using value and take new parameter
            //check is it use in 0th parameter...
            if (memberExpressionName == _parameters[0].Name)
            {
                parameterExpression = _parameters[0];
            }
            //check is it use in 1th parameter
            else if (memberExpressionName == _parameters[1].Name)
            {
                parameterExpression = _parameters[1];
            }
            else
                //there is no more arguments -> something went wrong
                throw new Exception();

            //One-element IEnumerable collection with name of member of parameter
            Expression[] arguments = new[] { Expression.Constant(ValuesDictKey) };
            Expression[] argument = new[] { Expression.Constant(ValueOfIndexesKey) };

            //Property of ThumbnailObject.this[uint key]
            PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item");
            //PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item", TO_bindingAttr);

            //Make expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
            IndexExpression IndexAccessExpr = Expression.MakeIndex(parameterExpression, TO_indekser, argument);

            //Convert above expression from ValueType to particular type of frontal value
            return Expression.Convert(IndexAccessExpr, node.Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new Exception("You cannot to use object method call to create model of object. Try to write this method (" + node.ToString() + ")as new lambda which uses only ValueType member(s) of object.");
        }
    }
}