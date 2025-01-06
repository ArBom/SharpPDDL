using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace SharpPDDL
{
    public abstract class ObjectPDDL
    {
        readonly public string Name;

        readonly internal Type TypeOf1Class;
        readonly internal Int32 Hash1Class;
        internal List<string> usedMembers1Class;
        internal int? AllParamsOfAct1ClassPos = null;

        internal readonly Type TypeOf2Class = null;
        internal readonly Int32? Hash2Class = null;
        internal List<string> usedMembers2Class;
        internal int? AllParamsOfAct2ClassPos = null;

        internal abstract void CompleteClassPos(IReadOnlyList<Parametr> Parameters);

        protected ObjectPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class=null, Int32? Hash2Class=null)
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

        internal bool TXIndex<T>(T t, int XClass, IReadOnlyList<Parametr> listOfParams)
        {
            int HashXClass;

            switch (XClass)
            {
                case 1:
                    HashXClass = Hash1Class;
                    break;

                case 2:
                    if (Hash2Class.HasValue)
                        HashXClass = Hash2Class.Value;
                    else
                        return false;
                    break;

                default:
                    return false;
            }

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != HashXClass)
                    continue;

                if (t.Equals(listOfParams[index].Oryginal))
                {
                    switch (XClass)
                    {
                        case 1:
                            AllParamsOfAct1ClassPos = index;
                            break;

                        case 2:
                            AllParamsOfAct2ClassPos = index;
                            break;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
