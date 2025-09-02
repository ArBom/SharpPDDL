using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpPDDL
{
    internal abstract class ObjectPDDL
    {
        readonly public string Name;
        internal ElementInOnbjectPDDL[] Elements = null;

        protected ObjectPDDL(string Name, object[] ElementsInOnbjectPDDL)
        {
            if (String.IsNullOrEmpty(Name))
            {
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 18, GloCla.ResMan.GetString("C2"));
                throw new Exception(GloCla.ResMan.GetString("C2"));
            }

            this.Name = Name;

            if (ElementsInOnbjectPDDL is null)
                this.Elements = new ElementInOnbjectPDDL[0];
            else
            {
                this.Elements = new ElementInOnbjectPDDL[ElementsInOnbjectPDDL.Length];

                for (int i = 0; i != ElementsInOnbjectPDDL.Length; i++)
                    Elements[i] = new ElementInOnbjectPDDL(ElementsInOnbjectPDDL[i]);
            }
        }

        internal abstract void CompleteActinParams(IList<Parametr> Parameters);
       
        internal void CompleteClassPos(IReadOnlyList<Parametr> Parameters)
        {
            for (int i = 0; i != Elements.Length; i++)
            {
                ElementInOnbjectPDDL TempElementInOnbjectPDDL = Elements[i];
                bool indexed = false;

                for (int index = 0; index != Parameters.Count; index++)
                {
                    if (Parameters[index].HashCode != TempElementInOnbjectPDDL.HashClass)
                        continue;

                    if (TempElementInOnbjectPDDL.Object.Equals(Parameters[index].Oryginal))
                    {
                        TempElementInOnbjectPDDL.AllParamsOfActClassPos = index;
                        indexed = true;
                        break;
                    }
                }

                if (indexed)
                    continue;

                string ObjectPDDLtype = this.GetType().Name;
                string ExceptionMess = String.Format(GloCla.ResMan.GetString("C45"), TempElementInOnbjectPDDL.TypeOfClass, Name, ObjectPDDLtype);
                GloCla.Tracer?.TraceEvent(TraceEventType.Critical, 145, ExceptionMess);
                throw new Exception(ExceptionMess);
            }
        }
    }
}
