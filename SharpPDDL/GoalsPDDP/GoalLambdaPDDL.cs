using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    class GoalLambdaPDDL<T1> : ExpressionVisitor where T1 : class
    {
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(PossibleStateThumbnailObject), "PossibleThumbnailObject");
        readonly Type OryginalObjectType;
        readonly T1 OryginalObject;
        private readonly List<SingleTypeOfDomein> allTypes;
        Expression CheckingTheParametr;
        Expression<Predicate<PossibleStateThumbnailObject>> ModifeidLambda;

        public GoalLambdaPDDL(T1 oryginalObject, List<SingleTypeOfDomein> allTypes)
        {
            this.allTypes = allTypes;
            this.OryginalObjectType = typeof(T1);
            this.OryginalObject = oryginalObject;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrsEquals();
        }

        protected Expression CheckingTheParametrsEquals()
        {
            FieldInfo keyOfPrecursor = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredFields.First(df => df.Name == "Precursor");
            MemberExpression ThObPrecursor = Expression.MakeMemberAccess(_parameter, keyOfPrecursor);

            FieldInfo keyOfOriginalObj = typeof(ThumbnailObjectPrecursor<T1>).GetTypeInfo().DeclaredFields.First(df => df.Name == "OriginalObj");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(ThObPrecursor, keyOfOriginalObj);

            ConstantExpression ConType = Expression.Constant(OryginalObject, typeof(T1));
            return Expression.Equal(ThObOryginalType, ConType);
        }

        public GoalLambdaPDDL(Type oryginalObjectType, List<SingleTypeOfDomein> allTypes)
        {
            this.allTypes = allTypes;
            this.OryginalObjectType = oryginalObjectType;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrType();
        }

        protected Expression CheckingTheParametrType()
        {
            FieldInfo key = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredFields.First(df => df.Name == "OriginalObjType");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(_parameter, key);
            ConstantExpression ConType = Expression.Constant(OryginalObjectType, typeof(Type));
            return Expression.Equal(ThObOryginalType, ConType);
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

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (typeof(Predicate<T1>) != typeof(T))
                throw new Exception();

            if (node.Parameters.Count != 1)
                throw new Exception();

            var ModefNode = Visit(node.Body);
            var PPModifeidLambda = Expression.AndAlso(CheckingTheParametr, ModefNode);

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
                MemberInfo memberInfo = typeof(T1).GetMember(MemberName).First();
                if (!memberInfo.ReflectedType.IsValueType)
                    throw new Exception();

                //...and chech the type of it...
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            staticValue = (ValueType)typeof(T1).GetField(MemberName).GetValue(OryginalObject);
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            staticValue = (ValueType)typeof(T1).GetProperty(MemberName).GetValue(OryginalObject);
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            if (typeof(T1).GetMethod(MemberName).GetParameters().Count() != 0)
                                throw new Exception();

                            staticValue = (ValueType)typeof(T1).GetMethod(MemberName).Invoke(OryginalObject, null);
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

            throw new Exception();
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new Exception("You cannot to use object method call to create model of object. Try to write this method (" + node.ToString() + ")as new lambda which uses only ValueType member(s) of object.");
        }
    }
}
