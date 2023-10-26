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
        Func<ThumbnailObject, ThumbnailObject, bool?> CheckPDDP2;

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

            //ThumbnailObLambdaModif<T1> thumbnailObLambdaModif = new ThumbnailObLambdaModif<T1>();
            //var funcprim = thumbnailObLambdaModif.Visit(func);// as Expression<Predicate<ThumbnailObject>>;

            

            //Expression.Convert(func, Func<>);

            /*CheckPDDP2 = (Param1, Param2) =>
            {
                var V1null = Param1.predicates.Where(p => p.name == Name)?.First(p => p.type == TypeOf1Class)?.value;

                return V1null == V2null;
            };

            var a = func.Body;

            ParameterExpression Parameter = func.Parameters[0];

            if (Parameter.Type != typeof(T1))
            {
                throw new Exception();
            }

            var aaa = Parameter.Name;
            var t = func.ToString();*/
        }
    }

    public class ThumbnailObLambdaModif : ExpressionVisitor
    {
        private ReadOnlyCollection<ParameterExpression> _parameters;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _parameters = VisitAndConvert<ParameterExpression>(node.Parameters, "VisitLambda");

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
            /*if (node.Member.DeclaringType == typeof(TSource))
            {
                return Test2(node.Expression.ToString(), node.Member.Name);
            } 24,10,23 */

            /*if (node.NodeType == ExpressionType.MemberAccess)
            {
                var otherMember = (typeof(ThumbnailObject).GetField("dict")).GetValue(node.Member.Name);

            }

            return base.VisitMember(node);*/

            /*Type type = node.Type;

            return Test2(node.Expression.ToString(), node.Member.Name, type);*/

            ParameterExpression parameterExpression = Expression.Parameter(typeof(ThumbnailObject), node.Expression.ToString());

            var dicti = MemberExpression.PropertyOrField(parameterExpression, "dict");
            //Type dictionaryType = this.Target.GetType().GetField("dict").FieldType;
            Type dictionaryType = typeof(ThumbnailObject).GetField("dict").FieldType;

            PropertyInfo indexerProp = dictionaryType.GetProperty("Item");
            var dictKeyConstant = Expression.Constant(node.Member.Name);
            //this.Target.dict.Add(PropertyOrFieldKey, default(char));
            IndexExpression dictAccess = Expression.MakeIndex(dicti, indexerProp, new[] { dictKeyConstant });
            Expression dictAccess2 = Expression.Convert(dictAccess, node.Type);

            return dictAccess2;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return node;
        }
    }
}
