using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1c, T1p, T2c, T2p> : PreconditionPDDL<T1c, T1p> where T1p : class where T1c : T1p where T2p : class where T2c : T2p
    {
        internal T2c t2;
        readonly Expression<Predicate<T1p, T2p>> func;

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

        internal PreconditionPDDL(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func) : base(Name, ref obj1, obj2.GetType(), obj2.GetHashCode())
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
