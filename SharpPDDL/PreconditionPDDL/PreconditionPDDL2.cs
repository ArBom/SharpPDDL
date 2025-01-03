using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1c, T1p, T2c, T2p> : PreconditionPDDL
        where T2p : class
        where T1c : class, T1p 
        where T2c : class, T2p
    {
        protected readonly T1c t1;
        protected readonly T2c t2;

        internal PreconditionPDDL(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func) : base(Name, obj1.GetType(), obj1.GetHashCode(), obj2.GetType(), obj2.GetHashCode())
        {
            this.func = func;
            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            _ = memberofLambdaListerPDDL.Visit(func);
            this.usedMembers1Class = memberofLambdaListerPDDL.used[0];
            this.usedMembers2Class = memberofLambdaListerPDDL.used[1];
            this.t1 = obj1;
            this.t2 = obj2;
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(t1, 1, Parameters) == false)
                throw new Exception("There is no that param at list.");

            if (TXIndex(t2, 2, Parameters) == false)
                throw new Exception("There is no that param at list.");
        }

        internal override Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = { AllParamsOfAct1ClassPos.Value, AllParamsOfAct2ClassPos.Value };
            PreconditionLambdaModif preconditionLambdaModifList = new PreconditionLambdaModif(allTypes, ParamsIndexesInAction);
            CheckPDDP = (Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>>)preconditionLambdaModifList.Visit(this.func);
            return CheckPDDP;
        }
    }
}
