using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    class GoalObjectMember : IGoalObject
    {
        readonly object _OriginalObj;
        public object OriginalObj
        {
            get => _OriginalObj;
            set => throw new NotImplementedException();
        }

        readonly Type _OryginalObjType;
        public Type OriginalObjType => _OryginalObjType;

        readonly LambdaExpression InLambda;
        public Delegate GoalPDDL => InLambda.Compile();

        readonly DomainPDDL _NewPDDLdomain;
        public DomainPDDL NewPDDLdomain => _NewPDDLdomain;

        readonly bool _MigrateIntheEnd;
        public bool MigrateIntheEnd => false;

        internal GoalObjectMember(object originalObj, Type originalObjType, DomainPDDL newPDDLdomain, LambdaExpression lambda, bool MigrateIt)
        {
            this._OriginalObj = originalObj;
            this._OryginalObjType = originalObjType;
            this._NewPDDLdomain = newPDDLdomain;
            this.InLambda = lambda;
            this._MigrateIntheEnd = MigrateIt;
        }

        public LambdaExpression BuildGoalPDDP(DomainPDDL GoalOwner, GoalPDDL Owner)
        {
            ParameterExpression _param = InLambda.Parameters[0];
            //Checking the Oryginal Object Type of _parameter is like expected
            PropertyInfo keyOriginalObjType = typeof(ThumbnailObject).GetTypeInfo().DeclaredProperties.First(df => df.Name == "OriginalObjType");
            MemberExpression ThObOryginalType = Expression.MakeMemberAccess(_param, keyOriginalObjType);
            Expression TypeIs = Expression.TypeIs(ThObOryginalType, _OryginalObjType);
            BinaryExpression NewLambdaBody = Expression.AndAlso(TypeIs, InLambda.Body);
            LambdaExpression ToRet = Expression.Lambda(NewLambdaBody, _param);
            return ToRet;
        }
    }
}
