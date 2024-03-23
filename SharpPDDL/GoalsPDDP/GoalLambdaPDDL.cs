using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    class GoalLambdaPDDL<T1> : ExpressionVisitor where T1 : class
    {
        private readonly ParameterExpression _parameter = Expression.Parameter(typeof(PossibleStateThumbnailObject), "PossibleThumbnailObject");
        readonly Type OryginalObjectType;
        readonly T1 OryginalObject;
        private readonly List<SingleTypeOfDomein> allTypes;
        BinaryExpression CheckingTheParametr;

        public GoalLambdaPDDL(T1 oryginalObject, List<SingleTypeOfDomein> allTypes)
        {
            this.allTypes = allTypes;
            this.OryginalObjectType = typeof(T1);
            this.OryginalObject = oryginalObject;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrsEquals();
        }
        protected BinaryExpression CheckingTheParametrsEquals()
        {
            FieldInfo key = typeof(PossibleStateThumbnailObject).GetTypeInfo().DeclaredFields.First(df => df.Name == "Precursor");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(_parameter, key);

            FieldInfo key2 = typeof(ThumbnailObjectPrecursor<T1>).GetTypeInfo().DeclaredFields.First(df => df.Name == "OriginalObj");
            MemberExpression ThObOryginalType2 = Expression.MakeMemberAccess(ThObOryginalType, key2);

            ConstantExpression ConType = Expression.Constant(OryginalObject, typeof(T1));
            return Expression.Equal(ThObOryginalType2, ConType);
        }

        public GoalLambdaPDDL(Type oryginalObjectType, List<SingleTypeOfDomein> allTypes)
        {
            this.allTypes = allTypes;
            this.OryginalObjectType = oryginalObjectType;
            CheckConstructorParam();
            CheckingTheParametr = CheckingTheParametrType();
        }

        protected BinaryExpression CheckingTheParametrType()
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

        protected override Expression VisitLambda<T1>(Expression<T1> node)
        {
            if (node.Parameters.Count != 1)
                throw new Exception();

            throw new NotImplementedException();
        }
    }
}
