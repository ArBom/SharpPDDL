using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class ElementInOnbjectPDDL
    {
        internal readonly object Object;
        internal readonly Type TypeOfClass;
        internal readonly Int32 HashClass;
        internal List<string> usedMembersClass;
        internal int? AllParamsOfActClassPos;

        public ElementInOnbjectPDDL(Type TypeOfClass, Int32 HashClass)
        {
            this.TypeOfClass = TypeOfClass;
            this.HashClass = HashClass;
            this.usedMembersClass = new List<string>();
            this.AllParamsOfActClassPos = null;
        }

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
