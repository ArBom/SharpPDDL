using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    internal class GoalLambdaPDDL<T> : ExpressionVisitor where T : class
    {
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix);
        readonly Type OryginalObjectType;
        readonly T OryginalObject;
        private readonly List<SingleTypeOfDomein> allTypes;
        readonly Expression CheckingTheParametr;
        internal LambdaExpression ModifeidLambda;
        readonly ICollection<Expression<Predicate<T>>> GoalExpectations;

        public GoalLambdaPDDL(ICollection<Expression<Predicate<T>>> GoalExpectations, List<SingleTypeOfDomein> allTypes, T oryginalObject)
        {
            if (oryginalObject is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E17"), typeof(T));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 88, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            this.allTypes = allTypes;
            this.OryginalObjectType = oryginalObject.GetType();
            this.OryginalObject = oryginalObject;
            this.GoalExpectations = GoalExpectations;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrsEquals();
            CheckPredicates(GoalExpectations);
        }

        protected Expression CheckingTheParametrsEquals()
        {
            //Checking Oryginal Object Type
            PropertyInfo keyOfOriginalObjType = typeof(ThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObjType");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(_parameter, keyOfOriginalObjType);
            Expression TypeIs = Expression.Equal(ThObOryginalType, Expression.Constant(OryginalObjectType));

            //Checking equals of objects
            PropertyInfo keyOfOriginalObj = typeof(ThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObj");
            MemberExpression ThObPrecursor = Expression.MakeMemberAccess(_parameter, keyOfOriginalObj);
            ConstantExpression ConOrygObj = Expression.Constant(OryginalObject, typeof(T));
            Expression Equals = Expression.Call(typeof(Object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }), ConOrygObj, ThObPrecursor);

            //Return top connected as andalso expression
            return Expression.AndAlso(TypeIs, Equals);
        }

        public GoalLambdaPDDL(ICollection<Expression<Predicate<T>>> GoalExpectations, List<SingleTypeOfDomein> allTypes, Type oryginalObjectType)
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
            PropertyInfo keyOriginalObjType = typeof(ThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObjType");
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
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C24"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 89, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!allTypes.Any())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C25"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 90, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (OryginalObjectType is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C26"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 91, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!OryginalObjectType.IsClass)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E18"), OryginalObjectType.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 92, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }

        Expression CheckPredicates(ICollection<Expression<Predicate<T>>> GoalExpectations)
        {
            if (GoalExpectations is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E19"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 93, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            IEnumerator<Expression<Predicate<T>>> Enumerator = GoalExpectations.GetEnumerator();
            int GoalExpectationsCount = GoalExpectations.Count;

            //if its empty
            if (!Enumerator.MoveNext())
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E20"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 94, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            Expression CheckAllPreco = VisitLambda(Enumerator.Current);

            while (Enumerator.MoveNext())
                CheckAllPreco = Expression.AndAlso(CheckAllPreco, VisitLambda(Enumerator.Current));

            //To poniższe jest wykorzystywane dalej
            ModifeidLambda = Expression.Lambda(Expression.AndAlso(CheckingTheParametr, CheckAllPreco), _parameter);

            try
            {
                _ = ModifeidLambda.Compile();
            }
            catch
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C27"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 95, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            return ModifeidLambda;
        }

        protected override Expression VisitLambda<Tp>(Expression<Tp> node)
        {
            if (node.Parameters.Count != 1)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E21"), node.ToString());
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 96, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            return Visit(node.Body);
        }

        private SingleTypeOfDomein IdentifyParameterModel(MemberExpression node)
        {
            SingleTypeOfDomein ParameterModel = null;
            Type originalObjTypeCand = node.Expression.Type;
            do
            {
                IEnumerator<SingleTypeOfDomein> ModelsEnum = allTypes.Where(t => t.Type == originalObjTypeCand).GetEnumerator();

                if (ModelsEnum.MoveNext())
                    ParameterModel = ModelsEnum.Current;

                originalObjTypeCand = originalObjTypeCand.BaseType;
            }
            while (ParameterModel is null && !(originalObjTypeCand is null));

            if (ParameterModel is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C28"), node.Expression.Type);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 97, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            return ParameterModel;
        }

        private UnaryExpression ValueFromModel(ushort Value, Type NodeType)
        {
            Expression[] argument = new Expression[1] { Expression.Constant(Value, typeof(ushort)) };

            //Property of ThumbnailObject.this[uint key]
            PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item");

            //Make expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
            IndexExpression IndexAccessExpr = Expression.MakeIndex(_parameter, TO_indekser, argument);

            if (NodeType.IsValueType)
            {
                //Convert above expression from ValueType to particular type of frontal value
                return Expression.Convert(IndexAccessExpr, NodeType);
            }
            else
            {
                return Expression.Convert(IndexAccessExpr, typeof(IntPtr));
            }
        }

        private MemberExpression ValueFromOrygObjField(UnaryExpression unaryExpression, string MemberName)
        {
            FieldInfo FieldType = typeof(T).GetField(MemberName);

            if (!FieldType.IsInitOnly)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E24"), MemberName);
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 100, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!FieldType.FieldType.IsValueType && FieldType.FieldType != typeof(string))
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E25"), MemberName);
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 101, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            MemberExpression FieldAccess = Expression.Field(unaryExpression, FieldType);
            return FieldAccess;
        }

        private ConstantExpression ValueFromOrygObjProperty(UnaryExpression unaryExpression, string MemberName)
        {
            PropertyInfo PropertyType = typeof(T).GetProperty(MemberName);

            if (PropertyType.CanWrite || !PropertyType.CanRead)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E26"), MemberName);
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 102, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!PropertyType.PropertyType.IsValueType && PropertyType.PropertyType != typeof(string))
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E27"), MemberName);
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 103, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            Expression PropertyAccess = Expression.Property(unaryExpression, PropertyType);

            object value = (ValueType)PropertyType.GetValue(OryginalObject);
            ConstantExpression staticExValue = Expression.Constant(value, PropertyType.PropertyType);
            return staticExValue;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant)
                return node;

            if (node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberInfo NextMember = ((MemberExpression)(node.Expression)).Member;
                if (!NextMember.ReflectedType.IsClass)
                {
                    //todo;
                    var t = node;
                    //GloCla.Tracer?.TraceEvent(TraceEventType.Error, , GloCla.ResMan.GetString(""));
                }
                else
                {
                    if (NextMember.ReflectedType.IsAbstract)
                    {
                        //todo;
                        //GloCla.Tracer?.TraceEvent(TraceEventType.Error, , GloCla.ResMan.GetString(""));
                    }
                    else
                    {
                        int t = 5;
                    }
                }
            }

            SingleTypeOfDomein ParameterModel = IdentifyParameterModel(node);

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            IEnumerable<ushort> Values = ParameterModel.CumulativeValues.Where(v => v.Name == MemberName).Select(v => v.ValueOfIndexesKey);

            //thumbnailObj allows for it already
            if (Values.Any())
                return ValueFromModel(Values.First(), node.Type);
            //thumbnailObj ignoring it, but we check particular obj.
            else
            {
                //it will be check constant value of it
                MemberInfo keyOrygObj = typeof(ThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObj");
                Expression OrygObj = Expression.MakeMemberAccess(_parameter, keyOrygObj);
                UnaryExpression Converted = Expression.Convert(OrygObj, ParameterModel.Type);
                Expression staticExValue;

                //get the member...
                MemberInfo memberInfo = typeof(T).GetMember(MemberName).First();
                //...and check the type of it...
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        {
                            staticExValue = ValueFromOrygObjField(Converted, MemberName);
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            staticExValue = ValueFromOrygObjProperty(Converted, MemberName);
                            break;
                        }
                    default:
                        {
                            string ExceptionMess = String.Format(GloCla.ResMan.GetString("E22"), MemberName);
                            GloCla.Tracer?.TraceEvent(TraceEventType.Error, 98, ExceptionMess);
                            throw new Exception(ExceptionMess);
                        }
                }

                return Expression.Convert(staticExValue, node.Type);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            string ExceptionMess = String.Format(GloCla.ResMan.GetString("E23"), node.ToString());
            GloCla.Tracer?.TraceEvent(TraceEventType.Error, 99, ExceptionMess);
            throw new Exception(ExceptionMess);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value == null)
                return Expression.Constant(IntPtr.Zero, typeof(IntPtr));
            else
                return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);

            if (node.Left.Type.IsClass || node.Right.Type.IsClass)
                return Expression.MakeBinary(node.NodeType, left, right);
            else
                return node.Update(left, VisitAndConvert(node.Conversion, "VisitBinary"), right);
        }
    }
}