using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

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

        public SingleType(Type type, IReadOnlyList<Value> values)
        {
            this.Type = type;
            this.Values = new List<Value>();

            MemberInfo[] AllTypeMembers = type.GetMembers();

            if (values is null)
                return;

            foreach (Value value in values)
                if (AllTypeMembers.Any(allM => allM.Name == value.Name))
                    this.Values.Add(value);
        }
    }

    internal class SingleTypeOfDomein : SingleType
    {
        internal List<ValueOfThumbnail> CumulativeValues;
        new internal List<ValueOfThumbnail> Values;
        internal ushort[] ValuesKeys;

        internal SingleTypeOfDomein(Type type, List<ValueOfThumbnail> values) : base(type)
        {
            this.Values = values;
            this.CumulativeValues = new List<ValueOfThumbnail>();
        }

        internal SingleTypeOfDomein(SingleType singleType) : base(singleType.Type)
        {
            this.Values = new List<ValueOfThumbnail>();

            foreach (Value value in singleType.Values)
            {
                ValueOfThumbnail TempValueOfThumbnail = new ValueOfThumbnail((ValueOfParametr)value);
                Values.Add(TempValueOfThumbnail);
            }
            this.CumulativeValues = new List<ValueOfThumbnail>();
        }

        internal void CreateValuesKeys()
        {
            List<ushort> TempList = new List<ushort>();

            foreach (ValueOfThumbnail valueOfThumbnail in CumulativeValues)
                TempList.Add(valueOfThumbnail.ValueOfIndexesKey);

            TempList.Sort();
            ValuesKeys = TempList.ToArray();
        }
    }
}
