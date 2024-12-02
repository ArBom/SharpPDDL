using System;

namespace SharpPDDL
{
    internal class Value
    {
        readonly internal string Name;
        internal Type OwnerType;
        readonly internal Type Type;
        protected ushort _ValueOfIndexesKey = 0;

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

        internal Value(Value OthetValue)
        {
            this.Name = OthetValue.Name;
            this.OwnerType = OthetValue.OwnerType;
            this.Type = OthetValue.Type;
            this.IsField = OthetValue.IsField;

            this.IsInUse_PreconditionIn = OthetValue.IsInUse_PreconditionIn;
            this.IsInUse_EffectIn = OthetValue.IsInUse_EffectIn;
            this.IsInUse_EffectOut = OthetValue.IsInUse_EffectOut;
        }

        internal bool IsInUse
        {
            get { return ( _IsInUse_EffectIn || _IsInUse_EffectOut || _IsInUse_PreconditionIn || _IsInUse_ActionCostIn); }
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

        //In the beginning one premise it will be not in use
        protected bool _IsInUse_ActionCostIn = false;
        internal bool IsInUse_ActionCostIn
        {
            get { return _IsInUse_ActionCostIn; }
            set
            {
                //It can be change only for true
                if (value)
                    _IsInUse_ActionCostIn = true;
            }
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

    internal class ValueOfThumbnail : Value
    {
        internal ValueOfThumbnail(Value valueOfParametr) : base(valueOfParametr.Name, valueOfParametr.Type, valueOfParametr.OwnerType, valueOfParametr.IsField)
        {
            IsInUse_PreconditionIn = valueOfParametr.IsInUse_PreconditionIn;
            IsInUse_EffectIn = valueOfParametr.IsInUse_EffectIn;
            IsInUse_EffectOut = valueOfParametr.IsInUse_EffectOut;
        }
    }
}