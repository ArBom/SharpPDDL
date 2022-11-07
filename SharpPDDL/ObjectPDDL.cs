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

        /*
        protected int IndexOf1OnList(List<Parametr> listOfParams)
        {
            for (int listPos = 0; listPos != listOfParams.Count; listPos++)
            {
                if (listOfParams[listPos].HashCode != this.Hash1Class)
                    continue;

                if (!object.ReferenceEquals(listOfParams[listPos], this.Hash1Class)) //TODO tu coś chyba źle przyrównanie obiektu i jego hasha???
                    continue;

                return listPos;
            }

            throw new Exception(); //Brak na liście
        }*/

        internal abstract (int, int?) FindIndexesOnList(List<Parametr> listOfParams);

        internal ObjectPDDL(string Name)
        {
            if (String.IsNullOrEmpty(Name))
                throw (new Exception());//nazwa nie może być pusta
            else
                this.Name = Name;
        }
    }
}
