using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    abstract class ObjectPDDL
    {
        readonly internal string Name;

        readonly internal Type TypeOf1Class;
        readonly internal Int32 Hash1Class;

        internal ObjectPDDL(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw (new Exception());//nazwa nie może być pusta
            else
                this.Name = Name;
        }
    }
}
