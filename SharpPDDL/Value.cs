using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal abstract class Value
    {
        readonly internal string Name;
        internal Type OwnerType;

        //true for field, false for properties
        internal bool IsField;

        internal Value(string name, Type typeOfValue, Type typeOfOwner, bool isField)
        {
            if (!typeOfValue.IsValueType)
                throw new InvalidOperationException();

            this.Name = name;
            this.OwnerType = typeOfOwner;
            this.IsField = isField;
        }
    }

    internal class ValueOfParametr : Value
    {
        readonly int OryginalMemberHash;
        readonly internal Type Type;

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

        internal ValueOfParametr(string name, Type typeOfValue, Type typeOfOwner, bool isField) : base(name, typeOfValue, typeOfOwner, isField)
        {
            this._IsInUse = false;
            this.Type = typeOfValue;
        }
    }

    internal class ValueOfThumbnail : Value
    {
        protected ushort _ValueOfIndexesKey = 0;

        //internal ValueOfThumbnail(string name, Type typeOfValue, Type typeOfOwner, bool isField) : base(name, typeOfValue, typeOfOwner, isField) {}

        internal ValueOfThumbnail(ValueOfParametr valueOfParametr) : base(valueOfParametr.Name, valueOfParametr.Type, valueOfParametr.OwnerType, valueOfParametr.IsField) { }

        internal ushort ValueOfIndexesKey
        {
            get { return _ValueOfIndexesKey; }
            set
            {
                if (_ValueOfIndexesKey == 0)
                    _ValueOfIndexesKey = value;
            }
        }
    }
}
