using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpPDDL
{
    internal abstract class ObjectPDDL
    {
        readonly public string Name;
        internal ElementInOnbjectPDDL[] Elements = null;

        internal abstract void CompleteActinParams(IList<Parametr> Parameters);
        internal abstract void CompleteClassPos(IReadOnlyList<Parametr> Parameters);
       
        internal void CompleteClassPosAlt(IReadOnlyList<Parametr> Parameters)
        {
            throw new NotImplementedException(); //TODO

            for (int i = 0; i != Elements.Length; i++)
                if (TXIndexAlt(Elements[i], i, Parameters) == false)
                {
                    string ObjectPDDLtype = this.GetType().Name;
                    string ExceptionMess = String.Format(GloCla.ResMan.GetString("XD XD XD XD XD XD XD XD XD XD XD"), Elements[i].TypeOfClass, Name);
                    GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 1000000000, ExceptionMess);
                    throw new Exception(ExceptionMess);
                }

            bool TXIndexAlt<T>(T t, int i, IReadOnlyList<Parametr> listOfParams)
            {
                for (int index = 0; index != listOfParams.Count; index++)
                {
                    if (listOfParams[index].HashCode != Elements[i].HashClass)
                        continue;

                    if (Elements[i].Object.Equals(listOfParams[index].Oryginal))
                    {
                        Elements[i].AllParamsOfActClassPos = index;
                        return true;
                    }
                }

                return false;
            }
        }

        protected ObjectPDDL(string Name, object[] ElementsInOnbjectPDDL)
        {
            if (String.IsNullOrEmpty(Name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 18, GloCla.ResMan.GetString("C2"));
                throw new Exception(GloCla.ResMan.GetString("C2"));
            }

            this.Name = Name;

            if (!(ElementsInOnbjectPDDL is null))
            {
                this.Elements = new ElementInOnbjectPDDL[ElementsInOnbjectPDDL.Length];

                for (int i = 0; i != ElementsInOnbjectPDDL.Length; i++)
                    Elements[i] = new ElementInOnbjectPDDL(ElementsInOnbjectPDDL[i]);
            }
            else
                this.Elements = new ElementInOnbjectPDDL[0];
        }

        internal bool TXIndex<T>(T t, int XClass, IReadOnlyList<Parametr> listOfParams)
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
