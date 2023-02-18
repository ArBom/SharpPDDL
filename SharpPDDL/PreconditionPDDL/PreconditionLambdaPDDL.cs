using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    class PreconditionLambdaPDDL<T1> : PreconditionPDDL<T1>
    {
        internal PreconditionLambdaPDDL(string Name, ref T1 obj1, Expression<Predicate<T1>> func) : base(Name, ref obj1)
        {
            Predicate<T1> inPredComp = func.Compile();
            Check = (Param1, Param2, List) =>
            {
                if (Param1 == null)
                    return null;

                if (!(Param1 is T1))
                    return null;

                T1 t1 = Param1;
                return inPredComp(t1);
            };

            var a = func.Body;

            ParameterExpression Parameter = func.Parameters[0];

            if (Parameter.Type != typeof(T1))
            {
                throw new Exception();
            }

            var aaa = Parameter.Name;
            var t = func.ToString();
        }
    }

    public class ParameterTypeVisitor2<TSource> : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> _parameters;

        ParameterTypeVisitor2(ref ThumbnailObject target)
        {

        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameters?.FirstOrDefault(p => p.Name == node.Name) ??
                (node.Type == typeof(TSource) ? Expression.Parameter(typeof(ThumbnailObject), node.Name) : node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");
            return Expression.Lambda(Visit(node.Body), _parameters);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return node.Update(Visit(node.Left), VisitAndConvert(node.Conversion, "VisitBinary"), Visit(node.Right));
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType == typeof(TSource))
            {
                return Test2(node.Expression.ToString(), node.Member.Name);
            }
            return base.VisitMember(node);
        }

        public Expression Test2(string str, string PropertyOrFieldKey)
        {
            //ParameterExpression parameterExpression = Expression.Parameter(this.Target.GetType(), str);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(ThumbnailObject), str);

            var dicti = MemberExpression.PropertyOrField(parameterExpression, "dict");
            //Type dictionaryType = this.Target.GetType().GetField("dict").FieldType;
            Type dictionaryType = typeof(ThumbnailObject).GetField("dict").FieldType;

            PropertyInfo indexerProp = dictionaryType.GetProperty("Item");
            var dictKeyConstant = Expression.Constant(PropertyOrFieldKey);
            //this.Target.dict.Add(PropertyOrFieldKey, default(char));
            IndexExpression dictAccess = Expression.MakeIndex(dicti, indexerProp, new[] { dictKeyConstant });

            return dictAccess;
        }
    }
}
