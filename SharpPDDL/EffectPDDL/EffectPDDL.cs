﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal abstract class EffectPDDL : ObjectPDDL
    {
        internal abstract Func<ThumbnailObject, ThumbnailObject, KeyValuePair<ushort, ValueType>> BuildCheckPDDP(List<SingleTypeOfDomein> allTypes);
        internal Action<object, object> Effect;

        //Hashes[0] - destination; Hashes[1] - source (if exist);
        readonly int[] Hashes;
        readonly internal string DestinationMemberName;

        internal EffectPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null) : base(Name, TypeOf1Class, Hash1Class, TypeOf2Class, Hash2Class) { }

        internal static EffectPDDL Instance<T1>(string Name, ValueType newValue_Static, ref T1 destinationObj, Func<T1, ValueType> destinationMember) where T1 : class //przypisanie wartosci ze stałej
        {
            if (newValue_Static.GetType() != destinationMember.GetMethodInfo().ReturnType)
                throw new Exception("You cannot assign new value. Its another type.");

            return new EffectPDDL1<T1>(Name, newValue_Static, ref destinationObj, CreateExpression(destinationMember));
        }

        internal static EffectPDDL Instance<T1,T2>(string Name, ref T1 sourceObj1, Func<T1, ValueType> Source, ref T2 DestinationObj, Func<T2, ValueType> destinationMember) where T1 : class where T2 : class
        {
            if (Source.GetMethodInfo().ReturnType != destinationMember.GetMethodInfo().ReturnType)
                throw new Exception("You cannot assign new value. Its another type.");

            return new EffectPDDL2<T1, T2>(Name, ref sourceObj1, CreateExpression(Source), ref DestinationObj, CreateExpression(destinationMember));
        }

        internal static EffectPDDL Instance<T1, T2>(string Name, ref T1 sourceObj1, Func<T1, T2, ValueType> Source, ref T2 DestinationObj, Func<T2, ValueType> destinationMember) where T1 : class where T2 : class
        {
            if (Source.GetMethodInfo().ReturnType != destinationMember.GetMethodInfo().ReturnType)
                throw new Exception("You cannot assign new value. Its another type.");

            return new EffectPDDL2<T1, T2>(Name, ref sourceObj1, CreateExpression(Source), ref DestinationObj, CreateExpression(destinationMember));
        }
    }
}
