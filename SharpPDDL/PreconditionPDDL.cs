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
        /// Position of the first parameter in the list
        /// (Position of the second parameter in the list)
        /// </returns>
        internal abstract (Func<ThumbnailObject, ThumbnailObject, bool>, Func<dynamic, dynamic, bool?>, int, int?) BuildFunct(/*List<Parametr> listOfParams*/);

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
        protected Func<dynamic, dynamic, bool?> Check;

        internal PreconditionPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null) : base(Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class)
        {

        }

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

    internal class PreconditionPDDL<T1> : PreconditionPDDL
    {
        protected T1 t1;
        Expression<Predicate<T1>> func;

        protected int T1Index (List<Parametr> listOfParams)
        {
            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash1Class)
                    continue;

                if (ReferenceEquals(listOfParams[index], t1))
                    return index;
            }
            return -1;
        }

        internal override (Func<ThumbnailObject, ThumbnailObject, bool>, Func<dynamic, dynamic, bool?>, int, int?) BuildFunct(/*List<Parametr> listOfParams*/)
        {
            //this.CheckPDDP = a.ReduceAndCheck();
            return (CheckPDDP, Check, 0/*T1Index(listOfParams)*/, null);
        }

        protected PreconditionPDDL(string Name, ref T1 obj1, Type TypeOf2Class, Int32 HashOf2Class) : base(Name, obj1.GetType(), obj1.GetHashCode(), TypeOf2Class, HashOf2Class)
        {
            this.t1 = obj1;
        }

        internal PreconditionPDDL(string Name, ref T1 obj1, Expression<Predicate<T1>> func) : base(Name, obj1.GetType(), obj1.GetHashCode())
        {
            this.func = func;

            ThumbnailObLambdaModif thumbnailObLambdaModif = new ThumbnailObLambdaModif();
            thumbnailObLambdaModif.Visit(func);
            this.CheckPDDP = thumbnailObLambdaModif.ModifiedFunct;
            this.usedMembers1Class = thumbnailObLambdaModif.used[0];
            this.t1 = obj1;
        }
    }

    internal class PreconditionPDDL<T1, T2> : PreconditionPDDL<T1> where T1 : class where T2 : class
    {
        internal T2 t2;
        Expression<Predicate<T1, T2>> func;

        private int T2Index(List<Parametr> listOfParams)
        {
            if (!TypeOf2Class.IsClass)
                return -2;

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != Hash2Class)
                    continue;

                if (ReferenceEquals(listOfParams[index], t2))
                    return index;
            }
            return -1;
        }

        new protected (Func<ThumbnailObject, ThumbnailObject, bool>, Func<dynamic, dynamic, bool?>, int, int?) BuildFunct(List<Parametr> listOfParams)
        {
            return (CheckPDDP, Check, T1Index(listOfParams), T2Index(listOfParams));
        }

        internal PreconditionPDDL(string Name, ref T1 obj1, ref T2 obj2, Expression<Predicate<T1, T2>> func) : base(Name, ref obj1, obj2.GetType(), obj2.GetHashCode())
        {
            this.func = func;

            ThumbnailObLambdaModif thumbnailObLambdaModif = new ThumbnailObLambdaModif();
            thumbnailObLambdaModif.Visit(func);
            this.CheckPDDP = thumbnailObLambdaModif.ModifiedFunct;
            this.usedMembers1Class = thumbnailObLambdaModif.used[0];
            this.usedMembers2Class = thumbnailObLambdaModif.used[1];
            this.t1 = obj1;
            this.t2 = obj2;
        }
    }
}