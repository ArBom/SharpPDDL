using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpPDDL
{
    class EffectPDDLpointer<T1c, T1p, T2c, T2p> : EffectPDDL
        where T1p : class
        where T2p : class
        where T1c : class, T1p
        where T2c : class, T2p
    {
        internal EffectPDDLpointer(string Name, ref T1c DestinationObj, Expression<Func<T1p, T2p>> Destination, ref T2c SourceObj)
            : base(Name, Destination, new object[2] { DestinationObj, SourceObj }) { }

        internal EffectPDDLpointer(string Name, ref T1c DestinationObj, Expression<Func<T1p, T2p>> Destination)
            : base(Name, Destination, new object[1] { DestinationObj }) { }

        private string MutualPartOfConstructors(Expression DestinationFunct)
        {
            MemberofLambdaListerPDDL DestLambdaListerPDDL = new MemberofLambdaListerPDDL();
            DestLambdaListerPDDL.Visit(DestinationFunct);
            string NameOfDestinationMember = DestLambdaListerPDDL.used[0][0];

            if (Elements.Length != 1)
                if (!Elements[1].usedMembersClass.Exists(m => m == NameOfDestinationMember))
                    Elements[1].usedMembersClass.Add(NameOfDestinationMember);

            return NameOfDestinationMember;
        }

        override internal void CompleteActinParams(IList<Parametr> Parameters)
        {
            MemberofLambdaListerPDDL SourceLambdaListerPDDL = new MemberofLambdaListerPDDL();
            SourceLambdaListerPDDL.Visit(SourceFunc);

            Elements[0].usedMembersClass = new List<string>();
            //Elements[1].usedMembersClass = new List<string>();

            if (SourceFunc is Expression<Func<T1p, T2p, ValueType>>)
            {
                Elements[0].usedMembersClass = SourceLambdaListerPDDL.used[0];
                Elements[1].usedMembersClass = SourceLambdaListerPDDL.used[1];
            }
            else if (SourceFunc is Expression<Func<T2p, ValueType>>)
            {
                Elements[1].usedMembersClass = SourceLambdaListerPDDL.used[0];
            }

            this.DestinationMemberName = MutualPartOfConstructors(DestinationMember);

            //Tag destination parameter value as "IsInUse"
            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != Elements[0].HashClass)
                    continue;

                if (!parametr.Oryginal.Equals(Elements[0].Object))
                    continue;

                int ToTagIndex = parametr.values.FindIndex(v => v.Name == DestinationMemberName);
                parametr.values[ToTagIndex].IsInUse_EffectOut = true;

                foreach (string valueName in Elements[0].usedMembersClass)
                {
                    ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse_EffectIn = true;
                }

                parametr.UsedInEffect = true;
                break;
            }
        }

        internal override Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>> BuildEffectPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            ParameterExpression[] _parameters = new ParameterExpression[2];
            ushort Key = allTypes.First(t => t.Type == Elements[0].TypeOfClass).CumulativeValues.Where(v => v.Name == DestinationMemberName).Select(v => v.ValueOfIndexesKey).First();
            ConstantExpression FuncOutKey = Expression.Constant(Key, typeof(ushort));
            ParameterExpression DestinationPara = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix + Elements[0].AllParamsOfActClassPos.Value);
            _parameters[0] = DestinationPara;

            ParameterExpression SourcePara;
            Expression NewValue;

            if (Elements.Length == 1)
            {
                SourcePara = Expression.Parameter(typeof(ThumbnailObject), GloCla.EmptyName);
                NewValue = Expression.Constant(IntPtr.Zero, typeof(IntPtr));
            }
            else if (Elements.Length == 2)
            {
                SourcePara = Expression.Parameter(typeof(ThumbnailObject), GloCla.LamdbaParamPrefix + Elements[1].AllParamsOfActClassPos.Value);

                Expression[] argument = new[] { Expression.Constant((ushort)0, typeof(ushort)) };

                //Property of ThumbnailObject.this[uint key]
                PropertyInfo TO_indekser = typeof(ThumbnailObject).GetProperty("Item");

                NewValue = Expression.MakeIndex(SourcePara, TO_indekser, argument);
            }
            else
                throw new Exception();

            _parameters[1] = SourcePara;
            ConstructorInfo ResultType = typeof(KeyValuePair<ushort, ValueType>).GetConstructors()[0];
            UnaryExpression ConvertedNewValue = Expression.Convert(NewValue, typeof(ValueType));
            Expression[] paramOfNewExpr = { FuncOutKey, ConvertedNewValue };
            NewExpression expectedTypeExpression = Expression.New(ResultType, paramOfNewExpr);
            LambdaExpression ModifiedFunct = Expression.Lambda<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>(expectedTypeExpression, _parameters);

            try
            {
                _ = ModifiedFunct.Compile();
            }
            catch (Exception e)
            {
                throw new Exception();
            }

            return (Expression<Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>>>)ModifiedFunct;
        }
    }
}