﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1, T2> : PreconditionPDDL<T1> where T1 : class where T2 : class
    {
        internal T2 t2;
        readonly Expression<Predicate<T1, T2>> func;

        private void T2Index(IReadOnlyList<Parametr> listOfParams)
        {
            if (TypeOf2Class is null)
                return;

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash2Class)
                    continue;

                if (t2.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct2ClassPos = index;
                    return;
                }
            }

            throw new Exception("There is no that param at list.");
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            T1Index(Parameters);
            T2Index(Parameters);
        }

        internal override Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = { AllParamsOfAct1ClassPos.Value, AllParamsOfAct2ClassPos.Value };
            PreconditionLambdaModif preconditionLambdaModifList = new PreconditionLambdaModif(allTypes, ParamsIndexesInAction);
            CheckPDDP = (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>>)preconditionLambdaModifList.Visit(this.func);
            return CheckPDDP;
        }

        protected (Expression, Func<dynamic, dynamic, bool>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            return (CheckPDDP, Check, AllParamsOfAct1ClassPos.Value, AllParamsOfAct1ClassPos);
        }

        internal PreconditionPDDL(string Name, ref T1 obj1, ref T2 obj2, Expression<Predicate<T1, T2>> func) : base(Name, ref obj1, obj2.GetType(), obj2.GetHashCode())
        {
            this.func = func;
            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            _ = memberofLambdaListerPDDL.Visit(func);
            //todo sprawdzenie czy typy paramrtrów się zgadzają
            this.usedMembers1Class = memberofLambdaListerPDDL.used[0];
            this.usedMembers2Class = memberofLambdaListerPDDL.used[1];
            this.t2 = obj2;
        }
    }
}
