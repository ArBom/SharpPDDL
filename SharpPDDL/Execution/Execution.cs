using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal abstract class Execution
    {
        readonly protected string Name;
        internal Delegate Delegate;

        internal Execution(string Name) { this.Name = Name; }
        internal abstract Delegate CreateEffectDelegate(IReadOnlyList<Parametr> Parameters);

        protected int? Index<T>(T t, IReadOnlyList<Parametr> Parameters)
        {
            for (int index = 0; index != Parameters.Count; index++)
            {
                if (Parameters[index].HashCode != t.GetHashCode())
                    continue;

                if (t.Equals(Parameters[index].Oryginal))
                {
                    return index;
                }
            }

            return null;
        }
    }
}
