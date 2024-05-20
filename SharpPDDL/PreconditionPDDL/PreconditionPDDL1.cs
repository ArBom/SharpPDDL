using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1> : PreconditionPDDL where T1 : class
    {
        protected T1 t1;
        readonly Expression<Predicate<T1>> func;

        protected void T1Index(IReadOnlyList<Parametr> listOfParams)
        {
            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash1Class)
                    continue;

                if (t1.Equals(listOfParams[index].Oryginal))
                {
                    AllParamsOfAct1ClassPos = index;
                    return;
                }
            }

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

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters) => T1Index(Parameters);

        protected PreconditionPDDL(string Name, ref T1 obj1, Type TypeOf2Class, Int32 HashOf2Class) : base(Name, obj1.GetType(), obj1.GetHashCode(), TypeOf2Class, HashOf2Class)
        {
            this.t1 = obj1;
        }

        internal PreconditionPDDL(string Name, ref T1 obj1, Expression<Predicate<T1>> func) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.func = func;
            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            _ = memberofLambdaListerPDDL.Visit(func);
            this.usedMembers1Class = memberofLambdaListerPDDL.used[0];
            this.t1 = obj1;
        }
    }
}