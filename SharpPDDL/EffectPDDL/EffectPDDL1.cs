using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SharpPDDL
{
    class EffectPDDL1<T1c, T1p> : EffectPDDL
        where T1p : class 
        where T1c : class, T1p
    {
        internal EffectPDDL1(string Name, ref T1c obj1, Expression<Func<T1p, ValueType>> Destination, ValueType newValue)
            : base(Name, Destination, new object[1] { obj1 })
            => this.SourceFunc = Expression.Constant(newValue, newValue.GetType());

        override internal void CompleteActinParams(IList<Parametr> Parameters)
        {
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(DestinationMember);
            this.Elements[0].usedMembersClass = DestLambdaListerPDDL.used[0];
            this.DestinationMemberName = Elements[0].usedMembersClass[0];

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != Elements[0].HashClass)
                    continue;

                if (!parametr.Oryginal.Equals(Elements[0].Object))
                    continue;

                int ToTagIndex = parametr.values.FindIndex(v => v.Name == DestinationMemberName);
                parametr.values[ToTagIndex].IsInUse_EffectOut = true;

                parametr.UsedInEffect = true;
                break;
            }
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(Elements[0].Object, 1, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C17"), typeof(T1c).ToString(), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 80, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }

        internal override Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);

            ushort FuncOutKey = allTypes.First(t => t.Type == Elements[0].TypeOfClass).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            Expression FuncOutKeyExpression = Expression.Constant(FuncOutKey, typeof(ushort));

            Collection<ParameterExpression> parameterExpressions = new Collection<ParameterExpression>
            {
                Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix + Elements[0].AllParamsOfActClassPos.Value),
                Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName)
            };

            var ResultType = typeof(KeyValuePair<ushort, ValueType>).GetConstructors()[0];
            Expression[] param = { FuncOutKeyExpression, Expression.Convert(SourceFunc, typeof(ValueType)) };
            NewExpression expectedTypeExpression = Expression.New(ResultType, param);
            Expression ModifiedFunct = Expression.Lambda<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>(expectedTypeExpression, parameterExpressions);

            return (Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>)ModifiedFunct;
        }
    }
}
