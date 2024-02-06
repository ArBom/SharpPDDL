using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SharpPDDL
{
    internal class EffectPDDL
    {
        readonly string Name; 
        internal Action<ThumbnailObject, ThumbnailObject> ExecutePDDP;
        internal Func<dynamic, dynamic, EventHandler> Execute;

        //Hashes[0] - destination; Hashes[1] - source (if exist);
        readonly int[] Hashes;
        readonly Expression ConstantSource;

        internal EffectPDDL(string Name, ValueType Source, ref ValueType Destination) //przypisanie wartosci ze stałej
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception();

            if (Source.GetType() != Destination.GetType())
                throw new Exception();

            this.Name = Name;
            ConstantSource = Expression.Constant(Source);
            Hashes = new int[] { Destination.GetHashCode() };
        }

        internal EffectPDDL(string Name, ref ValueType Source, ref ValueType Destination) //przypisanie zewnętrzne
        {
            if (String.IsNullOrEmpty(Name))
                throw new Exception();

            if (Source.GetType() != Destination.GetType())
                throw new Exception();

            this.Name = Name;
            Hashes = new int[] { Destination.GetHashCode(), Source.GetHashCode() };
            this.ConstantSource = null;
        }

    }
}
