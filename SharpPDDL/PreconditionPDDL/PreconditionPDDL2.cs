using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1c, T1p, T2c, T2p> : PreconditionPDDL
        where T1p : class
        where T2p : class
        where T1c : class, T1p 
        where T2c : class, T2p
    {
        protected readonly T1c t1;
        protected readonly T2c t2;

        internal PreconditionPDDL(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func) 
            : base(Name, func, obj1.GetType(), obj1.GetHashCode(), obj2.GetType(), obj2.GetHashCode())
        {
            this.t1 = obj1;
            this.t2 = obj2;
        }

        override internal void CompleteActinParams(IList<Parametr> Parameters)
        {
            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            _ = memberofLambdaListerPDDL.Visit(func);
            this.usedMembers1Class = memberofLambdaListerPDDL.used[0];
            this.usedMembers2Class = memberofLambdaListerPDDL.used[1];

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != t1.GetHashCode())
                    continue;

                if (!(parametr.Oryginal.Equals(t1)))
                    continue;

                foreach (string valueName in usedMembers1Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse_PreconditionIn = true;
                }

                parametr.UsedInPrecondition = true;
                break;
            }

            foreach (Parametr parametr in Parameters)
            {
                if (parametr.HashCode != t2.GetHashCode())
                    continue;

                if (!(parametr.Oryginal.Equals(t2)))
                    continue;

                foreach (string valueName in usedMembers2Class)
                {
                    int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                    parametr.values[ToTagIndex].IsInUse_PreconditionIn = true;
                }

                parametr.UsedInPrecondition = true;
                break;
            }
        }

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            if (TXIndex(t1, 1, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C32"), typeof(T1c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 112, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (TXIndex(t2, 2, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C33"), typeof(T2c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 113, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }

        internal override Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = { AllParamsOfAct1ClassPos.Value, AllParamsOfAct2ClassPos.Value };
            PreconditionLambdaModif preconditionLambdaModifList = new PreconditionLambdaModif(allTypes, ParamsIndexesInAction);
            CheckPDDP = (Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>>)preconditionLambdaModifList.Visit(this.func);
            return CheckPDDP;
        }
    }
}
