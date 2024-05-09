using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class GoalLambdaPDDL<T> : ExpressionVisitor where T : class
    {
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(PossibleStateThumbnailObject), "ToCheckParam");
        readonly Type OryginalObjectType;
        readonly T OryginalObject;
        private readonly List<SingleTypeOfDomein> allTypes;
        Expression CheckingTheParametr;
        internal LambdaExpression ModifeidLambda; //TODO to skończyć
        List<Expression<Predicate<T>>> GoalExpectations;

        public GoalLambdaPDDL(List<Expression<Predicate<T>>> GoalExpectations, Type oryginalObjectType, List<SingleTypeOfDomein> allTypes, T oryginalObject = null)
        {
            this.allTypes = allTypes;
            this.OryginalObjectType = typeof(T);
            this.OryginalObject = oryginalObject;
            this.GoalExpectations = GoalExpectations;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrsEquals();
            CheckPredicates(GoalExpectations);
        }

        protected Expression CheckingTheParametrsEquals()
        {
            FieldInfo keyOfPrecursor = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredFields.First(df => df.Name == "Precursor");
            MemberExpression ThObPrecursor = Expression.MakeMemberAccess(_parameter, keyOfPrecursor);
            FieldInfo keyOfOriginalObj = typeof(ThumbnailObjectPrecursor<T>).GetTypeInfo().DeclaredFields.First(df => df.Name == "OriginalObj");
            MemberExpression ThObOryginal = Expression.MakeMemberAccess(ThObPrecursor, keyOfOriginalObj);

            ConstantExpression ConType = Expression.Constant(OryginalObject, typeof(T));

            return Expression.Call(typeof(Object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }), ConType, ThObOryginal);
        }

        public GoalLambdaPDDL(List<Expression<Predicate<T>>> GoalExpectations, List<SingleTypeOfDomein> allTypes)
        {
            this.allTypes = allTypes;
            this.OryginalObjectType = typeof(T);
            this.GoalExpectations = GoalExpectations;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrType();
            CheckPredicates(GoalExpectations);
        }

        protected Expression CheckingTheParametrType()
        {
            FieldInfo key = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredFields.First(df => df.Name == "OriginalObjType");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(_parameter, key);
            ConstantExpression ConType = Expression.Constant(OryginalObjectType, typeof(Type));
            return Expression.Call(ThObOryginalType, typeof(Type).GetMethod("IsAssignableFrom", new Type[] { typeof(Type) }), ConType);
        }

        protected void CheckConstructorParam()
        {
            if (allTypes is null)
                throw new Exception();

            if (allTypes.Count == 0)
                throw new Exception();

            if (OryginalObjectType is null)
                throw new Exception();

            if (!OryginalObjectType.IsClass)
                throw new Exception();
        }

        Expression CheckPredicates(List<Expression<Predicate<T>>> GoalExpectations)
        {
            if (GoalExpectations is null)
                throw new Exception();

            int GoalExpectationsCount = GoalExpectations.Count;

            if (GoalExpectationsCount == 0)
                throw new Exception();

            Expression CheckAllPreco = VisitLambda(GoalExpectations[0]);

            if (GoalExpectationsCount == 1)
                return CheckAllPreco;

            for (int i = 1; i!= GoalExpectationsCount; i++)
                CheckAllPreco = Expression.AndAlso(CheckAllPreco, VisitLambda(GoalExpectations[i]));

            return CheckAllPreco;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.Parameters.Count != 1)
                throw new Exception();

            var ModefNode = Visit(node.Body);
            ModifeidLambda = Expression.Lambda(Expression.AndAlso(CheckingTheParametr, ModefNode), _parameter);

            try
            {
                _ = ModifeidLambda.Compile();
            }
            catch
            {
                throw new Exception("New func cannot be compilated.");
            }

            return ModifeidLambda;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant)
                return node;

            SingleTypeOfDomein ParameterModel = allTypes.Where(t => t.Type == node.Expression.Type).First();

            if (ParameterModel is null)
                throw new Exception();

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;
            ushort? ValueOfIndexesKey = ParameterModel.CumulativeValues.Where(v => v.Name == MemberName)?.Select(v => v.ValueOfIndexesKey).First();

            //thumbnailObj allows for it already
            if (ValueOfIndexesKey.HasValue)
            {
                Expression[] argument = new[] { Expression.Constant(ValueOfIndexesKey.Value) };

                //Property of ThumbnailObject.this[uint key]
                PropertyInfo TO_indekser = typeof(PossibleStateThumbnailObject).GetProperty("Item");

                //Make expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
                IndexExpression IndexAccessExpr = Expression.MakeIndex(_parameter, TO_indekser, argument);

                //Convert above expression from ValueType to particular type of frontal value
                return Expression.Convert(IndexAccessExpr, node.Type);
            }
            //thumbnailObj ignoring it, but we check particular obj.
            else if (OryginalObject != null)
            {
                //it will be check constant value of it
                ValueType staticValue;

                //get the member...
                MemberInfo memberInfo = typeof(T).GetMember(MemberName).First();
                if (!memberInfo.ReflectedType.IsValueType)
                    throw new Exception();

                //...and check the type of it...
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            staticValue = (ValueType)typeof(T).GetField(MemberName).GetValue(OryginalObject);
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            staticValue = (ValueType)typeof(T).GetProperty(MemberName).GetValue(OryginalObject);
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            if (typeof(T).GetMethod(MemberName).GetParameters().Count() != 0)
                                throw new Exception();

                            staticValue = (ValueType)typeof(T).GetMethod(MemberName).Invoke(OryginalObject, null);
                            break;
                        }
                    default:
                        {
                            throw new Exception();
                        }
                }

                Expression staticExValue = Expression.Constant(staticValue);
                return Expression.Convert(staticExValue, node.Type);
            }
            else
                throw new Exception();
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new Exception("You cannot to use object method call to create model of object. Try to write this method (" + node.ToString() + ")as new lambda which uses only ValueType member(s) of object.");
        }
    }
}
