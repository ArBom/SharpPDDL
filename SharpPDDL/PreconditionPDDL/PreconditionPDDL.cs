using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpPDDL
{
    abstract internal class PreconditionPDDL : ObjectPDDL
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listOfParams"></param>
        /// <returns>
        /// Function that checks the condition of the PDDL object(s)
        /// Function that checks the condition of the object(s)
        /// </returns>
        //internal abstract (Func<ThumbnailObject, ThumbnailObject, bool>, Func<dynamic, dynamic, bool> ) TakeFunct();

        /// <summary>
        /// It's check if PDDL object(s) fulfil requirement (of this precondition) to do action.
        /// </summary>
        /// <returns>
        /// TRUE if so, FALSE if not, NULL if its incorrect
        /// </returns>
        protected Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>> CheckPDDP;

        internal abstract Expression<Func<PossibleStateThumbnailObject, PossibleStateThumbnailObject, bool>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes, IReadOnlyList<Parametr> Parameters);

        internal PreconditionPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null) : base(Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class) { }

        internal static PreconditionPDDL Instance<T1c, T1p>(string Name, ref T1c obj1, Expression<Predicate<T1p>> func) 
            where T1p : class 
            where T1c : T1p
        {
            return new PreconditionPDDL<T1c, T1p>(Name, ref obj1, func);
        }

        internal static PreconditionPDDL Instance<T1c, T1p, T2c, T2p>(string Name, ref T1c obj1, ref T2c obj2, Expression<Predicate<T1p, T2p>> func) 
            where T1p : class 
            where T2p : class 
            where T1c : T1p 
            where T2c : T2p
        {
            return new PreconditionPDDL<T1c, T1p, T2c, T2p>(Name, ref obj1, ref obj2, func);
        }
    }

    public delegate bool Predicate<in T1, in T2>(T1 arg1, T2 arg2);
}