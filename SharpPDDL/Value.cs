using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal class Value
    {
        readonly internal string Name;
        readonly internal Type Type;
        internal Type OwnerType;

        //true for field, false for properties
        readonly internal bool IsField;
        internal ValueType value;

        //In the beginning one premise it will be not in use
        private bool _IsInUse;

        internal bool IsInUse
        {
            get { return _IsInUse; }
            set
            {
                //It can be change only for true
                if (value)
                    _IsInUse = true;
            }
        }

        internal Value(string name, Type typeOfValue, Type typeOfOwner, bool isField)
        {
            if (!typeOfValue.IsValueType)
                throw new InvalidOperationException();

            this.Name = name;
            this.Type = typeOfValue;
            this.OwnerType = typeOfOwner;
            this.IsField = isField;

            this._IsInUse = false;
        }
    }

    internal class ValueOfParametr : Value
    {
        readonly int OryginalMemberHash;

        internal ValueOfParametr(string name, Type typeOfValue, Type typeOfOwner, bool isField) : base(name, typeOfValue, typeOfOwner, isField)
        {
            
        }
    }

    internal class ValueOfThumbnail : Value
    {

        internal ValueOfThumbnail(string name, Type typeOfValue, Type typeOfOwner, bool isField) : base(name, typeOfValue, typeOfOwner, isField)
        {

        }
    }
}
