using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal class ThumbnailObLambdaModif : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> _parameters;
        public List<string>[] used;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            used = new List<string>[2];
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");

            if (_parameters.Count() == 0)
            {
                //its no sens
            }

            if (_parameters.Count() == 1)
            {
                string NameOfNewOne = _parameters.First().Name == "null" ? "null2" : "null";
                List<ParameterExpression> parameters = _parameters.ToList<ParameterExpression>();
                parameters.Add(Expression.Parameter(typeof(ThumbnailObject), NameOfNewOne));
                _parameters = parameters.AsReadOnly();
            }

            var ret = Expression.Lambda(Visit(node.Body), _parameters);

            return ret;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var a = Expression.Parameter(typeof(ThumbnailObject), node.Name);
            return a;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            return node.Update(left, VisitAndConvert(node.Conversion, "VisitBinary"), right);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(ThumbnailObject), node.Expression.ToString());
            string MemberName = node.Member.Name;

            //TODO adding expression in use to the list

            var dicti = MemberExpression.PropertyOrField(parameterExpression, "dict");
            Type dictionaryType = typeof(ThumbnailObject).GetField("dict").FieldType;

            PropertyInfo indexerProp = dictionaryType.GetProperty("Item");
            var dictKeyConstant = Expression.Constant(MemberName);
            IndexExpression dictAccess = Expression.MakeIndex(dicti, indexerProp, new[] { dictKeyConstant });
            Expression dictAccess2 = Expression.Convert(dictAccess, node.Type);

            return dictAccess2;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new Exception("You cannot to use object method call to create model of object. Try to write this method (" + node.ToString() + ")as new lambda which uses only ValueType member(s) of object.");
        }
    }
}