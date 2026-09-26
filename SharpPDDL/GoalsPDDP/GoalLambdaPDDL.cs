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
        private List<ParameterExpression> parameters = new List<ParameterExpression> { Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix) };
        private readonly GoalPDDL Owner;
        readonly Type OryginalObjectType;
        readonly T OryginalObject;
        private readonly DomainPDDL GoalOwner;
        readonly Expression CheckingTheParametr;
        internal LambdaExpression ModifeidLambda;
        readonly ICollection<Expression<Predicate<T>>> GoalExpectations;

        public GoalLambdaPDDL(DomainPDDL GoalOwner, GoalPDDL Owner, ICollection<Expression<Predicate<T>>> GoalExpectations, T oryginalObject)
        {
            if (oryginalObject is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E17"), typeof(T));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 88, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            this.GoalOwner = GoalOwner;
            this.Owner = Owner;
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
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(parameters[0], keyOfOriginalObjType);
            Expression TypeIs = Expression.Equal(ThObOryginalType, Expression.Constant(OryginalObjectType));

            //Checking equals of objects
            PropertyInfo keyOfOriginalObj = typeof(ThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObj");
            MemberExpression ThObPrecursor = Expression.MakeMemberAccess(parameters[0], keyOfOriginalObj);
            ConstantExpression ConOrygObj = Expression.Constant(OryginalObject, typeof(T));
            Expression Equals = Expression.Call(typeof(Object).GetMethod("Equals", new Type[] { typeof(object), typeof(object) }), ConOrygObj, ThObPrecursor);

            //Return top connected as andalso expression
            return Expression.AndAlso(TypeIs, Equals);
        }

        public GoalLambdaPDDL(DomainPDDL GoalOwner, GoalPDDL Owner, ICollection<Expression<Predicate<T>>> GoalExpectations, Type oryginalObjectType)
        {
            this.GoalOwner = GoalOwner;
            this.Owner = Owner;
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
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(parameters[0], keyOriginalObjType);
            Expression TypeIs = Expression.TypeIs(ThObOryginalType, OryginalObjectType);

            //Checking is the Oryginal Object Type of _parameter is assignable from expected
            ConstantExpression ConstTypeExpr = Expression.Constant(OryginalObjectType, typeof(Type));
            Expression IsAssignableFromExpression = Expression.Call(ConstTypeExpr, typeof(Type).GetMethod("IsAssignableFrom", new Type[] { typeof(Type) }), ThObOryginalType);

            //return top connected as OrEle expression
            return Expression.OrElse(TypeIs, IsAssignableFromExpression);
        }

        protected void CheckConstructorParam()
        {
            if (GoalOwner.types.allTypes is null)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C24"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 89, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (!GoalOwner.types.allTypes.Any())
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

            //Try to compile it and return
            ModifeidLambda = Expression.Lambda(Expression.AndAlso(CheckingTheParametr, CheckAllPreco), parameters[0]);

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
                IEnumerator<SingleTypeOfDomein> ModelsEnum = GoalOwner.types.allTypes.Where(t => t.Type == originalObjTypeCand).GetEnumerator();

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
            IndexExpression IndexAccessExpr = Expression.MakeIndex(parameters[0], TO_indekser, argument);

            if (NodeType.IsValueType)
            {
                //Convert above expression from ValueType to particular type of frontal value
                return Expression.Convert(IndexAccessExpr, NodeType);
            }

            return Expression.Convert(IndexAccessExpr, typeof(IntPtr));
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

        private Expression VisitMember_ChangeParam(MemberExpression node)
        {
            MemberInfo NextMember = ((MemberExpression)(node.Expression)).Member;
            if (!NextMember.ReflectedType.IsClass)
            {
                //todo;
                //GloCla.Tracer?.TraceEvent(TraceEventType.Error, , GloCla.ResMan.GetString(""));
                throw new Exception();
            }

            if (NextMember.ReflectedType.IsAbstract)
            {
                //todo;
                //GloCla.Tracer?.TraceEvent(TraceEventType.Error, , GloCla.ResMan.GetString(""));
                throw new Exception();
            }

            Expression nodeExpression = node.Expression;
            Expression NodePrefix = VisitMember((MemberExpression)nodeExpression);

            string newParamName = nodeExpression.ToString().Replace('.', '_');

            //used for object added to domain - representand by thumbnail
            ParameterExpression newParamInside = Expression.Parameter(typeof(ThumbnailObject), newParamName);

            //used for object unadded to domain - take oryginal
            ParameterExpression newParamOutside = Expression.Parameter(((MemberExpression)node).Member.ReflectedType, newParamName);

            //Index nr of the other param at list
            int TheOtherParamNr;

            if (!parameters.Any(p => p.Name == newParamName))
            {
                parameters.Add(newParamInside);
                Expression constTrue = Expression.Constant(true, typeof(bool));
                Type newParamOutsideType = Expression.Parameter(((MemberExpression)node).Expression.Type).Type;
                ConstantExpression TypeExpr = Expression.Constant(newParamOutsideType, typeof(Type));

                MemberInfo keyOrygObjType = typeof(ThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObjType");
                Expression OrygObjType = Expression.MakeMemberAccess(newParamInside, keyOrygObjType);
                BinaryExpression checkType = Expression.MakeBinary(ExpressionType.Equal, OrygObjType, TypeExpr);

                LambdaExpression LambdaRetTrue = Expression.Lambda(constTrue, newParamOutside);

                GoalObjectMember goalObjectMember = new GoalObjectMember(null, ((MemberExpression)node).Member.ReflectedType, GoalOwner, LambdaRetTrue, false);
                Owner.GoalObjects.Add(goalObjectMember);
                TheOtherParamNr = Owner.GoalObjects.IndexOf(goalObjectMember);
            }
            else
                TheOtherParamNr = parameters.FindIndex(p => p.Name == newParamName);

            ParameterExpression FirstParam = (ParameterExpression)((IndexExpression)RemoveConvert(NodePrefix)).Object;
            ParameterExpression[] parameterExpressions = { FirstParam, parameters[TheOtherParamNr] };

            UnaryExpression NewParamInsideValueFromModel = ValueFromModel(0, nodeExpression.Type, newParamInside);
            BinaryExpression binaryExpression = Expression.Equal(NodePrefix, NewParamInsideValueFromModel);
            LambdaExpression labelExpression = Expression.Lambda(binaryExpression, parameterExpressions);
            int FirstParamNr = parameters.IndexOf(FirstParam);
            Owner.concatenatedConditions.Add(new ConcatenatedCondition(labelExpression, FirstParamNr, TheOtherParamNr));

            SingleTypeOfDomein ParameterModel = IdentifyParameterModel(node);

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            MemberExpression NewParamOutsideMembAcc = Expression.MakeMemberAccess(newParamOutside, node.Member);
            return VisitMember(NewParamOutsideMembAcc);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.NodeType == ExpressionType.Constant)
                return node;

            if (node.Expression.NodeType != ExpressionType.Parameter && node.Expression.Type.IsClass)
                return VisitMember_ChangeParam(node);

            SingleTypeOfDomein ParameterModel = IdentifyParameterModel(node);

            //its name of member of Parameter: Parameter => lambda(Parameter.Member) ; in these example string("Member")
            string MemberName = node.Member.Name;

            IEnumerable<ushort> Values = ParameterModel.CumulativeValues.Where(v => v.Name == MemberName).Select(v => v.ValueOfIndexesKey);

            //thumbnailObj allows for it already
            if (Values.Any())
                return ValueFromModel(Values.First(), node.Type);
            //thumbnailObj ignoring it, but we check particular obj.

            //it will be check constant value of it
            string ParamName = ((ParameterExpression)(node.Expression)).Name;
            ParameterExpression DefaultParam = parameters.FirstOrDefault(p => p.Name == ParamName);
            DefaultParam = DefaultParam is null ? parameters[0] : DefaultParam; 

            MemberInfo keyOrygObj = typeof(ThumbnailObject).GetTypeInfo().DeclaredMembers.First(df => df.Name == "OriginalObj");
            Expression OrygObj = Expression.MakeMemberAccess(DefaultParam, keyOrygObj);
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

            return node;
        }

        private UnaryExpression ValueFromModel(ushort Value, Type NodeType, ParameterExpression parameter)
        {
            Expression[] argument = new Expression[1] { Expression.Constant(Value, typeof(ushort)) };

            //Property of ThumbnailObject.this[uint key]
            PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item");

            //Make expression: from new parameter of ThumbnailObject type (parameterExpression) use indekser (TO_indekser) and take from it ValueType element with key (arguments), like frontal Member name
            IndexExpression IndexAccessExpr = Expression.MakeIndex(parameter, TO_indekser, argument);

            if (NodeType.IsValueType)
            {
                //Convert above expression from ValueType to particular type of frontal value
                return Expression.Convert(IndexAccessExpr, NodeType);
            }

            //Convert above to pointer
            return Expression.Convert(IndexAccessExpr, typeof(IntPtr));
        }

        protected Expression RemoveConvert(Expression node)
        {
            Expression ToRet = node;

            while (ToRet.NodeType == ExpressionType.Convert)
            {
                UnaryExpression n = (UnaryExpression)node;
                ToRet = n.Operand;
            }

            return ToRet;
        }

        protected string ParamName(Expression node)
        {
            Expression RetNode = RemoveConvert(node);

            if (RetNode.NodeType == ExpressionType.Parameter)
                return ((ParameterExpression)RetNode).Name;

            if (RetNode.NodeType != ExpressionType.MemberAccess)
                return null;

            Expression nodeExpression = ((MemberExpression)RetNode).Expression;
            nodeExpression = RemoveConvert(nodeExpression);

            if (nodeExpression.NodeType == ExpressionType.Parameter)
                return ((ParameterExpression)nodeExpression).Name;

            if (nodeExpression.NodeType == ExpressionType.Constant)
                return null;

            Expression NextMember = ((MemberExpression)nodeExpression).Expression;
            return ParamName(NextMember);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression left = Visit(node.Left);
            Expression right = Visit(node.Right);

            string LParam = ParamName(left);
            string RParam = ParamName(right);

            if (LParam != RParam)
            {
                int LParamNr = parameters.FindIndex(p => p.Name == LParam);
                int RParamNr = parameters.FindIndex(p => p.Name == RParam);

                BinaryExpression newret = Expression.MakeBinary(node.NodeType, left, right);

                ParameterExpression[] parameterExpressions = { parameters[LParamNr], parameters[RParamNr] };

                LambdaExpression labelExpression = Expression.Lambda(newret, parameterExpressions);

                Owner.concatenatedConditions.Add(new ConcatenatedCondition(labelExpression, LParamNr, RParamNr));

                return Expression.Constant(true, typeof(bool));
            }

            if (node.Left.Type.IsClass || node.Right.Type.IsClass)
                return Expression.MakeBinary(node.NodeType, left, right);            
            
            return node.Update(left, VisitAndConvert(node.Conversion, "VisitBinary"), right);
        }
    }
}