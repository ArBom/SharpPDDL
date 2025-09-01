using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class ElementInOnbjectPDDL
    {
        internal readonly object Object;
        internal Type TypeOfClass => Object.GetType();
        internal Int32 HashClass => Object.GetHashCode();
        internal List<string> usedMembersClass;
        internal int? AllParamsOfActClassPos;

        public ElementInOnbjectPDDL(object Object)
        {
            if (Object.GetType().IsValueType)
                throw new Exception();

            this.Object = Object;
            this.usedMembersClass = new List<string>();
            this.AllParamsOfActClassPos = null;
        }
    }
}
