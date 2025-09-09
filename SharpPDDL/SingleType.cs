using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace SharpPDDL
{
    internal class SingleType
    {
        internal readonly Type Type;
        internal List<Value> Values;

        protected SingleType(Type type)
        {
            this.Type = type;
        }

        internal SingleType(Type type, IReadOnlyList<Value> values)
        {
            this.Type = type;
            this.Values = new List<Value>();

            if (values is null)
                return;

            MemberInfo[] AllTypeMembers = type.GetMembers();

            foreach (Value value in values)
                if (AllTypeMembers.Any(allM => allM.Name == value.Name))
                    this.Values.Add(value);
        }
    }

    internal class SingleTypeOfDomein : SingleType
    {
        internal List<Value> CumulativeValues;
        new internal List<Value> Values;
        internal ushort[] ValuesKeys;
        internal bool NeedToTypeCheck = true;

        internal SingleTypeOfDomein(Type type, List<Value> values) : base(type)
        {
            this.Values = values;
            this.CumulativeValues = new List<Value>();
        }

        internal SingleTypeOfDomein(SingleType singleType) : base(singleType.Type)
        {
            this.Values = new List<Value>(singleType.Values);       
            this.CumulativeValues = new List<Value>();
        }

        internal void CreateValuesKeys()
            => ValuesKeys = CumulativeValues.Select(CV => CV.ValueOfIndexesKey).OrderBy(VOIK => VOIK).ToArray();
    }
}
