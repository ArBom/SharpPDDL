using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class GoalLambdaPDDL<T> : ExpressionVisitor where T : class
    {
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(PossibleStateThumbnailObject), ExtensionMethods.LamdbaParamPrefix);
        readonly Type OryginalObjectType;
        readonly T OryginalObject;
        private readonly List<SingleTypeOfDomein> allTypes;
        Expression CheckingTheParametr;
        internal LambdaExpression ModifeidLambda;
        List<Expression<Predicate<T>>> GoalExpectations;

        public GoalLambdaPDDL(List<Expression<Predicate<T>>> GoalExpectations, List<SingleTypeOfDomein> allTypes, T oryginalObject)
        {
            if (oryginalObject is null)
                throw new Exception();
            
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
            //Checking Oryginal Object Type
            PropertyInfo keyOfOriginalObjType = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObjType");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(_parameter, keyOfOriginalObjType);
            Expression TypeIs = Expression.TypeIs(ThObOryginalType, OryginalObjectType);

            //Checking equals of objects
            PropertyInfo keyOfOriginalObj = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObj");
            MemberExpression ThObPrecursor = Expression.MakeMemberAccess(_parameter, keyOfOriginalObj);
            ConstantExpression ConOrygObj = Expression.Constant(OryginalObject, typeof(T));
            Expression Equals = Expression.Call(typeof(Object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }), ConOrygObj, ThObPrecursor);

            //Return top connected as andalso expression
            return Expression.AndAlso(TypeIs, Equals);
        }

        public GoalLambdaPDDL(List<Expression<Predicate<T>>> GoalExpectations, List<SingleTypeOfDomein> allTypes, Type oryginalObjectType)
        {
            this.allTypes = allTypes;
            this.OryginalObjectType = oryginalObjectType;
            this.OryginalObject = null;
            this.GoalExpectations = GoalExpectations;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrType();
            CheckPredicates(GoalExpectations);
        }

        protected Expression CheckingTheParametrType()
        {
            //Checking the Oryginal Object Type of _parameter is like expected
            PropertyInfo keyOriginalObjType = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObjType");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(_parameter, keyOriginalObjType);
            Expression TypeIs = Expression.TypeIs(ThObOryginalType, OryginalObjectType);

            //Checking is the Oryginal Object Type of _parameter is assignable from expected
            ConstantExpression ConstTypeExpr = Expression.Constant(OryginalObjectType, typeof(Type));
            Expression IsAssignableFromExpression = Expression.Call(ConstTypeExpr, typeof(Type).GetMethod("IsAssignableFrom", new Type[] { typeof(Type) }), ThObOryginalType);

            //return top connected as OrEle expression
            return Expression.OrElse(TypeIs, IsAssignableFromExpression);
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

            if (GoalExpectationsCount != 1)
                for (int i = 1; i!= GoalExpectationsCount; i++)
                    CheckAllPreco = Expression.AndAlso(CheckAllPreco, VisitLambda(GoalExpectations[i]));

            //To poniższe jest wykorzystywane dalej
            ModifeidLambda = Expression.Lambda(Expression.AndAlso(CheckingTheParametr, CheckAllPreco), _parameter);

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

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.Parameters.Count != 1)
                throw new Exception();

            return Visit(node.Body);
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

            IEnumerable<ushort> Values = ParameterModel.CumulativeValues.Where(v => v.Name == MemberName).Select(v => v.ValueOfIndexesKey);

            //thumbnailObj allows for it already
            if (Values.Count() != 0)
            {
                Expression[] argument = new[] { Expression.Constant(Values.First()) };

                //Property of ThumbnailObject.this[uint key]
                PropertyInfo TO_indekser = typeof(PossibleStateThumbnailObject).GetProperty("Item");

                //Make expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
                IndexExpression IndexAccessExpr = Expression.MakeIndex(_parameter, TO_indekser, argument);

                //Convert above expression from ValueType to particular type of frontal value
                return Expression.Convert(IndexAccessExpr, node.Type);
            }
            //thumbnailObj ignoring it, but we check particular obj.
            else
            {
                //it will be check constant value of it
                MemberInfo keyOrygObj = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObj");
                Expression OrygObj = Expression.MakeMemberAccess(_parameter, keyOrygObj);
                Expression Converted = Expression.Convert(OrygObj, ParameterModel.Type);

                //get the member...
                MemberInfo memberInfo = typeof(T).GetMember(MemberName).First();

                Expression staticExValue;

                //...and check the type of it...
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            FieldInfo FieldType = typeof(T).GetField(MemberName);

                            if(!FieldType.IsInitOnly)
                                throw new Exception(MemberName + " have to be: constant / readonly / used as action argument");

                            if (!FieldType.FieldType.IsValueType && FieldType.FieldType != typeof(string))
                                throw new Exception("Variable used in goal checking have to be string or ValueType");

                            Expression FieldAccess = Expression.Field(Converted, FieldType);

                            staticExValue = FieldAccess;
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            PropertyInfo PropertyType = typeof(T).GetProperty(MemberName);

                            if (PropertyType.CanWrite || !PropertyType.CanRead)
                                throw new Exception(MemberName + " cannot be changed in time of programm run or have to be used as action argument");

                            if (!PropertyType.PropertyType.IsValueType && PropertyType.PropertyType != typeof(string))
                                throw new Exception("Variable used in goal checking have to be string or ValueType");

                            Expression PropertyAccess = Expression.Property(Converted, PropertyType);

                            object value = (ValueType)PropertyType.GetValue(OryginalObject);
                            staticExValue = Expression.Constant(value, PropertyType.PropertyType);
                            break;
                        }
                    default:
                        {
                            throw new Exception();
                        }
                }

                return Expression.Convert(staticExValue, node.Type);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new Exception("You cannot to use object method call to create model of object. Try to write this method (" + node.ToString() + ")as new lambda which uses only ValueType member(s) of object.");
        }
    }
}
