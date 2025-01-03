﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1c, T1p> : PreconditionPDDL 
        where T1p : class
        where T1c : class, T1p
    {
        protected readonly T1c t1;

        internal PreconditionPDDL(string Name, ref T1c obj1, Expression<Predicate<T1p>> func) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.func = func;
            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            _ = memberofLambdaListerPDDL.Visit(func);
            this.usedMembers1Class = memberofLambdaListerPDDL.used[0];
            this.t1 = obj1;
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(t1, 1, Parameters) == false)
                throw new Exception("There is no that param at list.");
        }

        internal override Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = { AllParamsOfAct1ClassPos.Value };
            PreconditionLambdaModif preconditionLambdaModif = new PreconditionLambdaModif(allTypes, ParamsIndexesInAction);
            CheckPDDP = (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>>)preconditionLambdaModif.Visit(this.func);
            return CheckPDDP;
        }
    }
}