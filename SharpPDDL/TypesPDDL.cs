using System;
using System.Linq;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class SingleType
    {
        internal readonly Type Type;
        public List<Value> Values;

        public SingleType(Type type)
        {
            this.Type = type;
            //this.predicates = new List<PredicatePDDL>();
        }

        /// <returns>List of Interfaces and classes inherited</returns>
        internal IReadOnlyList<Type> InheritedTypes()
        {
            List<Type> ToReturn = Type.GetInterfaces().ToList<Type>();

            Type typeUp = Type;
            while (typeUp != typeof(object))
            {
                ToReturn.Add(typeUp);
                typeUp = typeUp.BaseType;
            }

            return ToReturn;
        }
    }

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
