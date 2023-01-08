using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class SingleType
    {
        public readonly Type type;
        public List<PredicatePDDL> predicates;

        public SingleType(Type type)
        {
            this.type = type;
            this.predicates = new List<PredicatePDDL>();
        }
    }

    /*internal class Tree
    {
        public KnotOfTree root;
        public List<KnotOfTree> list = new List<KnotOfTree>();
    }*/

    public class TypesPDDL
    {
        internal List<SingleType> allTypes = new List<SingleType>();

        public void AddTypes(Type classes1, params Type[] classesN)
        {
            Type[] classes = new Type[classesN.Length + 1];
            classes[0] = classes1;
            classesN.CopyTo(classes, 1);

            foreach (var c in classes)
            {
                if (!c.IsClass)
                {
                    throw new Exception(""); //musi być klasa
                }

                if (allTypes.Exists(l => l.GetType() == c))
                    throw new Exception(""); //Taki typ został już dodany

                SingleType tempSingleType = new SingleType(c);
                allTypes.Add(tempSingleType);
            }
        }
    }
}
