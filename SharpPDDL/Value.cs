using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPDDL
{
    internal class Value
    {
        readonly internal string Name;
        readonly Type Type;

        //true for field, false for properties
        readonly internal bool IsField;

        internal ValueType value;

        //In the beginning one premise it will be not in use
        private bool _IsInUse;

        internal bool IsInUse
        {
            get { return IsInUse; }
            set
            {
                //It can be change only for true
                if (value)
                    IsInUse = true;
            }
        }

        internal Value(string name, Type type, bool isField)
        {
            if (!type.IsValueType)
                throw new InvalidOperationException();

            this.Name = name;
            this.Type = type;
            this.IsField = isField;

            this.IsInUse = false;
        }
    }
}
