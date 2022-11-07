using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace SharpPDDL
{
    public struct PredicatePDDL
    {
        readonly internal string Name;
        readonly Type TypeOf1Class;
        readonly Type TypeOf2Class;

        internal PredicatePDDL(string Name, Type TypeOf1Class, Type TypeOf2Class = null)
        {
            this.Name = Name;
            this.TypeOf1Class = TypeOf1Class;
            this.TypeOf2Class = TypeOf2Class;
        }
    }
}