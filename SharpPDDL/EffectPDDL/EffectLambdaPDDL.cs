﻿using System;
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
        public Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>> ModifiedFunct;
        private readonly int[] ParamsIndexesInAction;
        readonly List<SingleTypeOfDomein> allTypes;
        readonly Expression FuncOutKey = null;

        public EffectLambdaPDDL(List<SingleTypeOfDomein> allTypes, int[] paramsIndexesInAction, uint funcOutKey)
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
            this.FuncOutKey = Expression.Constant(funcOutKey, typeof(uint));
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            OldParameters = node.Parameters;
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

        private string NewParamName(string OldNodeName)
        {
            var param = OldParameters.First(p => p.Name == OldNodeName);
            int index = OldParameters.IndexOf(param);
            return "o" + index;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            string NewParamname = NewParamName(node.Name);
            return Expression.Parameter(typeof(ThumbnailObject), NewParamname);
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
            PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item");

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