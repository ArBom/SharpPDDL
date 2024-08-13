using System;
using System.Collections.Generic;

namespace SharpPDDL
{
    internal abstract class Value
    {
        readonly internal string Name;
        internal Type OwnerType;
        readonly internal Type Type;

        //true for field, false for properties
        internal bool IsField;

        internal Value(string name, Type typeOfValue, Type typeOfOwner, bool isField)
        {
            if (!typeOfValue.IsValueType)
                throw new InvalidOperationException();

            this.Name = name;
            this.OwnerType = typeOfOwner;
            this.Type = typeOfValue;
            this.IsField = isField;
        }

        internal bool IsInUse
        {
            get { return ( _IsInUse_EffectIn || _IsInUse_EffectOut || _IsInUse_PreconditionIn); }
        }

        //In the beginning one premise it will be not in use
        protected bool _IsInUse_PreconditionIn = false;
        internal bool IsInUse_PreconditionIn
        {
            get { return _IsInUse_PreconditionIn; }
            set
            {
                //It can be change only for true
                if (value)
                    _IsInUse_PreconditionIn = true;
            }
        }

        //In the beginning one premise it will be not in use
        protected bool _IsInUse_EffectIn = false;
        internal bool IsInUse_EffectIn
        {
            get { return _IsInUse_EffectIn; }
            set
            {
                //It can be change only for true
                if (value)
                    _IsInUse_EffectIn = true;
            }
        }

        //In the beginning one premise it will be not in use
        protected bool _IsInUse_EffectOut = false;
        internal bool IsInUse_EffectOut
        {
            get { return _IsInUse_EffectOut; }
            set
            {
                //It can be change only for true
                if (value)
                    _IsInUse_EffectOut = true;
            }
        }
    }

    internal class ValueOfParametr : Value
    {
        //In the beginning one premise it will be not in use
        private bool _IsInUse = false;

        internal new bool IsInUse
        {
            get { return _IsInUse; }
            set
            {
                //It can be change only for true
                if (value)
                    _IsInUse = true;
            }
        }

        internal ValueOfParametr(string name, Type typeOfValue, Type typeOfOwner, bool isField) : base(name, typeOfValue, typeOfOwner, isField) { }
    }

    internal class ValueOfThumbnail : Value
    {
        protected ushort _ValueOfIndexesKey = 0;

        internal ValueOfThumbnail(ValueOfParametr valueOfParametr) : base(valueOfParametr.Name, valueOfParametr.Type, valueOfParametr.OwnerType, valueOfParametr.IsField)
        {
            IsInUse_PreconditionIn = valueOfParametr.IsInUse_PreconditionIn;
            IsInUse_EffectIn = valueOfParametr.IsInUse_EffectIn;
            IsInUse_EffectOut = valueOfParametr.IsInUse_EffectOut;
        }

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
