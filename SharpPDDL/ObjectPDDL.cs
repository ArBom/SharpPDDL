using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpPDDL
{
    internal abstract class ObjectPDDL
    {
        readonly public string Name;
        internal ElementInOnbjectPDDL[] Elements = null;

        internal readonly Type TypeOf1Class;
        internal readonly Int32 Hash1Class;
        internal List<string> usedMembers1Class;
        internal int? AllParamsOfAct1ClassPos = null;

        internal readonly Type TypeOf2Class = null;
        internal readonly Int32? Hash2Class = null;
        internal List<string> usedMembers2Class;
        internal int? AllParamsOfAct2ClassPos = null;

        internal readonly Type TypeOf3Class = null;
        internal readonly Int32? Hash3Class = null;
        internal List<string> usedMembers3Class;
        internal int? AllParamsOfAct3ClassPos = null;

        internal abstract void CompleteActinParams(IList<Parametr> Parameters);
        internal abstract void CompleteClassPos(IReadOnlyList<Parametr> Parameters);

        protected ObjectPDDL(string Name, object[] ElementsInOnbjectPDDL)
        {
            if (String.IsNullOrEmpty(Name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 18, GloCla.ResMan.GetString("C2"));
                throw new Exception(GloCla.ResMan.GetString("C2"));
            }

            this.Name = Name;
            this.Elements = new ElementInOnbjectPDDL[ElementsInOnbjectPDDL.Length];

            for (int i = 0; i != ElementsInOnbjectPDDL.Length; i++)
                Elements[i] = new ElementInOnbjectPDDL(ElementsInOnbjectPDDL[i]);
        }

        protected ObjectPDDL(string Name, Type TypeOf1Class, Int32 Hash1Class, Type TypeOf2Class = null, Int32? Hash2Class = null, Type TypeOf3Class = null, Int32? Hash3Class = null)
        {
            if (String.IsNullOrEmpty(Name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 18, GloCla.ResMan.GetString("C2"));
                throw new Exception(GloCla.ResMan.GetString("C2"));
            }

            this.Name = Name;

            //////////////////////////

            this.TypeOf1Class = TypeOf1Class;
            this.Hash1Class = Hash1Class;
            this.usedMembers1Class = new List<string>();

            if (TypeOf2Class != null)
            {
                this.TypeOf2Class = TypeOf2Class;
                this.Hash2Class = Hash2Class;
                this.usedMembers2Class = new List<string>();
            }
            else
                return;

            if (TypeOf3Class != null)
            {
                this.TypeOf3Class = TypeOf3Class;
                this.Hash3Class = Hash3Class;
                this.usedMembers3Class = new List<string>();
            }

            ///////////////////////

            if (TypeOf3Class != null)
            {
                Elements = new ElementInOnbjectPDDL[3];

                Elements[2] = new ElementInOnbjectPDDL(TypeOf3Class, Hash3Class.Value);
            }

            if (TypeOf2Class != null)
            {
                if (Elements is null)
                    Elements = new ElementInOnbjectPDDL[2];

                Elements[1] = new ElementInOnbjectPDDL(TypeOf2Class, Hash2Class.Value);
            }

            if (Elements is null)
                Elements = new ElementInOnbjectPDDL[1];

            Elements[0] = new ElementInOnbjectPDDL(TypeOf1Class, Hash1Class);
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

                case 3:
                    if (Hash3Class.HasValue)
                        HashXClass = Hash3Class.Value;
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

                        case 3:
                            AllParamsOfAct3ClassPos = index;
                            break;
                    }

                    return true;
                }
            }

            return false;
        }

        internal bool TXIndexALT<T>(T t, int XClass, IReadOnlyList<Parametr> listOfParams)
        {
            if (Elements.Length < XClass--)
                return false;

            int HashXClass = Elements[XClass].HashClass;

            for (int index = 0; index != listOfParams.Count; index++)
            {
                if (listOfParams[index].HashCode != HashXClass)
                    continue;

                if (t.Equals(listOfParams[index].Oryginal))
                {
                    Elements[XClass].AllParamsOfActClassPos = index;
                    return true;
                }
            }

            return false;
        }
    }
}
