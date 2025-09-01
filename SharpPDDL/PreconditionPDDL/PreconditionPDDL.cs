using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Diagnostics;

namespace SharpPDDL
{
    public delegate bool Predicate<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate bool Predicate<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);

    abstract internal class PreconditionPDDL : ObjectPDDL
    {
        internal readonly Expression func;

        /// <summary>
        /// It's check if PDDL object(s) fulfil requirement (of this precondition) to do action.
        /// </summary>
        /// <returns>
        /// TRUE if so, FALSE if not, NULL if its incorrect
        /// </returns>
        protected Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> CheckPDDP;

        internal virtual Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters)
        {
            CompleteClassPos(Parameters);
            int[] ParamsIndexesInAction = Elements.Select(el => el.AllParamsOfActClassPos.Value).ToArray();
            PreconditionLambdaModif preconditionLambdaModifList = new PreconditionLambdaModif(allTypes, ParamsIndexesInAction);
            CheckPDDP = (Expression<Func<ThumbnailObject, ThumbnailObject, ThumbnailObject, bool>>)preconditionLambdaModifList.Visit(this.func);
            return CheckPDDP;
        }

        protected PreconditionPDDL(string Name, Expression func, object[] ElementsInOnbjectPDDL)
            : base(Name, ElementsInOnbjectPDDL)
            => this.func = func;

        internal static PreconditionPDDL Instance<T1c, T1p>(string Name, List<Parametr> Parameters, List<PreconditionPDDL> Preconditions, ref T1c obj1, Expression<Predicate<T1p>> func) 
            where T1p : class 
            where T1c : class, T1p
        {
            CheckExistPreconditionName(Preconditions, Name);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref obj1);
            PreconditionPDDL <T1c, T1p> NewPreconditionPDDL = new PreconditionPDDL<T1c, T1p>(Name, ref obj1, func);
            Preconditions?.Add(NewPreconditionPDDL);
            return NewPreconditionPDDL;
        }

        internal static PreconditionPDDL Instance<T1c, T1p, T2c, T2p>(string Name, List<Parametr> Parameters, List<PreconditionPDDL> Preconditions, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func) 
            where T1p : class 
            where T2p : class 
            where T1c : class, T1p 
            where T2c : class, T2p
        {
            CheckExistPreconditionName(Preconditions, Name);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref obj1);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref obj2);
            PreconditionPDDL<T1c, T1p, T2c, T2p> NewPreconditionPDDL = new PreconditionPDDL<T1c, T1p, T2c, T2p>(Name, ref obj1, ref obj2, func);
            Preconditions?.Add(NewPreconditionPDDL);
            return NewPreconditionPDDL;
        }

        internal static PreconditionPDDL Instance<T1c, T1p, T2c, T2p, T3c, T3p>(string Name, List<Parametr> Parameters, List<PreconditionPDDL> Preconditions, ref T1c obj1, ref T2c obj2, ref T3c obj3, Expression<Predicate<T1p, T2p, T3p>> func)
            where T1p : class
            where T2p : class
            where T3p : class
            where T1c : class, T1p
            where T2c : class, T2p
            where T3c : class, T3p
        {
            CheckExistPreconditionName(Preconditions, Name);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref obj1);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref obj2);
            Parametr.GetTheInstance_TryAddToList(Parameters, ref obj3);
            PreconditionPDDL<T1c, T1p, T2c, T2p, T3c, T3p> NewPreconditionPDDL = new PreconditionPDDL<T1c, T1p, T2c, T2p, T3c, T3p>(Name, ref obj1, ref obj2, ref obj3, func);
            Preconditions?.Add(NewPreconditionPDDL);
            return NewPreconditionPDDL;
        }

        protected static void CheckExistPreconditionName(List<PreconditionPDDL> Preconditions, string Name)
        {
            if (Preconditions is null)
                return;

            if (String.IsNullOrEmpty(Name))
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E31"));
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 109, ExceptionMess);
                throw new Exception(ExceptionMess);
            }

            if (Preconditions.Exists(precondition => precondition.Name == Name))
            {
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("E32"), Name);
                GloCla.Tracer?.TraceEvent(TraceEventType.Error, 110, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }

        internal override void CompleteActinParams(IList<Parametr> Parameters)
        {
            MemberofLambdaListerPDDL memberofLambdaListerPDDL = new MemberofLambdaListerPDDL();
            _ = memberofLambdaListerPDDL.Visit(func);
            ElementInOnbjectPDDL CurrentElOfLoop;

            for (int i = 0; i != Elements.Count(); i++)
            {
                CurrentElOfLoop = Elements[i];
                CurrentElOfLoop.usedMembersClass = memberofLambdaListerPDDL.used[i];

                foreach (Parametr parametr in Parameters)
                {
                    if (parametr.HashCode != CurrentElOfLoop.Object.GetHashCode())
                        continue;

                    if (!(parametr.Oryginal.Equals(CurrentElOfLoop.Object)))
                        continue;

                    foreach (string valueName in CurrentElOfLoop.usedMembersClass)
                    {
                        int ToTagIndex = parametr.values.FindIndex(v => v.Name == valueName);
                        parametr.values[ToTagIndex].IsInUse_PreconditionIn = true;
                    }

                    parametr.UsedInPrecondition = true;
                    break;
                }
            }
        }
    }
}