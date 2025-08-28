using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SharpPDDL
{
    internal class PreconditionPDDL<T1c, T1p, T2c, T2p, T3c, T3p> : PreconditionPDDL
    where T1p : class
    where T2p : class
    where T3p : class
    where T1c : class, T1p
    where T2c : class, T2p
    where T3c : class, T3p
    {
        internal PreconditionPDDL(string Name, ref T1c obj1, ref T2c obj2, ref T3c obj3, Expression<Predicate<T1p, T2p, T3p>> func)
            : base(Name, func, new object[3] { obj1, obj2, obj3 })
            => Expression.Empty();

        override internal void CompleteActinParams(IList<Parametr> Parameters) 
            => CompleteActinParamsALT(Parameters);

        internal override void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            throw new NotImplementedException();
            /*if (TXIndex(t1, 1, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString(""), typeof(T1c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, , ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (TXIndex(t2, 2, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString(""), typeof(T2c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, , ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (TXIndex(t3, 3, Parameters) == false)
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString(""), typeof(T3c), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, , ExceptionMess);
                throw new Exception(ExceptionMess);
            }*/
        }

        internal override Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = { AllParamsOfAct1ClassPos.Value, AllParamsOfAct2ClassPos.Value, AllParamsOfAct3ClassPos.Value };
            PreconditionLambdaModif preconditionLambdaModifList = new PreconditionLambdaModif(allTypes, ParamsIndexesInAction);
            CheckPDDP = (Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>>)preconditionLambdaModifList.Visit(this.func);
            return CheckPDDP;
        }
    }
}