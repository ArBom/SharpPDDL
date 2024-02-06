using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
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
        protected Func<ThumbnailObject, ThumbnailObject, bool> CheckPDDP;

        /// <summary>
        /// It's check if object(s) fulfil requirement (of this precondition) to do action.
        /// </summary>
        /// <returns>
        /// TRUE if so, FALSE if not, NULL if its incorrect
        /// </returns>
        protected Func<dynamic, dynamic, bool> Check;

        internal Func<ThumbnailObject, ThumbnailObject, bool> BuildCheckPDDP (Dictionary<ushort, Value> keyValuePairs)
        {

            return null;
        }

        internal PreconditionPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null) : base(Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class) { }

        internal static PreconditionPDDL Instance<T1>(string Name, ref T1 obj1, Expression<Predicate<T1>> func) where T1 : class
        {
            return new PreconditionPDDL<T1>(Name, ref obj1, func);
        }

        internal static PreconditionPDDL Instance<T1, T2>(string Name, ref T1 obj1, ref T2 obj2, Expression<Predicate<T1, T2>> func) where T1 : class where T2 : class
        {
            return new PreconditionPDDL<T1, T2>(Name, ref obj1, ref obj2, func);
        }
    }

    public delegate bool Predicate<in T1, in T2>(T1 arg1, T2 arg2);
}