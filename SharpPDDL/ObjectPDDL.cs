using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    abstract class ObjectPDDL
    {
        readonly internal string Name;

        readonly internal Type TypeOf1Class;
        readonly internal Int32 Hash1Class;
        internal List<string> usedMembers1Class;
        internal int? AllParamsOfAct1ClassPos = null;

        internal readonly Type TypeOf2Class = null;
        internal readonly Int32? Hash2Class = null;
        internal List<string> usedMembers2Class;
        internal int? AllParamsOfAct2ClassPos = null;

        internal abstract void CompleteClassPos(IReadOnlyList<Parametr> Parameters);

        internal ObjectPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class=null, Int32? Hash2Class=null)
        {
            //nazwa nie może być pusta
            if (String.IsNullOrEmpty(Name))
                throw new Exception();

            this.Name = Name;
            this.TypeOf1Class = TypeOf1Class;
            this.Hash1Class = Hash1Class;
            this.usedMembers1Class = new List<string>();

            if (TypeOf2Class!=null)
            {
                this.TypeOf2Class = TypeOf2Class;
                this.Hash2Class = Hash2Class;
                this.usedMembers2Class = new List<string>();
            }
        }
    }
}
