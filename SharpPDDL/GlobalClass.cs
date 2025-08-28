using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace SharpPDDL
{
    internal static class GloCla
    {
        /// <returns>List of Interfaces, List of Base Type; from orygilal type ToReturnBaseTypes[0], to object</returns>
        internal static (IReadOnlyList<Type> Interfaces, IReadOnlyList<Type> Types, IReadOnlyList<Type> TypesAndInterfaces) InheritedTypes(this Type type)
        {
            List<Type> ToReturnInterfaces = type.GetInterfaces().ToList();
            List<Type> ToReturnBaseTypes = new List<Type>();
            Type typeUp = type;
            while (typeUp != typeof(object))
            {
                ToReturnBaseTypes.Add(typeUp);
                typeUp = typeUp.BaseType;
            }
            List<Type> ToReturnAllTypes = new List<Type>();
            ToReturnAllTypes.AddRange(ToReturnBaseTypes);
            ToReturnAllTypes.AddRange(ToReturnInterfaces);

            return (ToReturnInterfaces, ToReturnBaseTypes, ToReturnAllTypes);
        }

        internal static string LamdbaParamPrefix => "o";
        internal static string SpecialFuncPrefix => "!";
        internal static string EmptyName => "empty";
        internal static string PlanToAcceptation => "Plan to acceptation";

        internal static ResourceManager ResMan = new ResourceManager("SharpPDDL.Properties.Resources", Assembly.GetExecutingAssembly());
        internal static TraceSource Tracer = null;
    }
}
