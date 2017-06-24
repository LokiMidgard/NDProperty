using System;

namespace NDProperty
{
    [Flags]
    public enum NDPropertySettings
    {
        None = 0,
        /// <summary>
        /// Normaly the OnPropertyChanged handler will only be called if old and new value are not equal. Using this setting it will even called if new and old value are the same.
        /// </summary>
        CallOnChangedHandlerOnEquals = 1 << 1,
        ParentReference = 1 << 2,
        ReadOnly = 1 << 3,
        Inherited = 1 << 4

    }
    public class NDReadOnlyProperty<TValue, TType> : IInternalNDReadOnlyProperty where TType : class
    {
        public bool Inherited => Settigns.HasFlag(NDPropertySettings.Inherited);
        public NDPropertySettings Settigns { get; }
        public NullTreatment NullTreatment { get; }

        public TValue DefaultValue { get; }

        internal NDReadOnlyProperty(NullTreatment nullTreatment, TValue defaultValue, NDPropertySettings settigns)
        {
            Settigns = settigns;
            DefaultValue = defaultValue;
            NullTreatment = nullTreatment;
        }

        public bool Equals(NDReadOnlyProperty<TValue, TType> obj)
        {
            var other = GetReadonly(obj);
            var me = GetReadonly(this);
            if (ReferenceEquals(me, this) && ReferenceEquals(obj, other))
                return ReferenceEquals(me, other);
            return me.Equals(other);
        }
        public override bool Equals(object obj)
        {
            if (obj is NDReadOnlyProperty<TValue, TType> p)
                return Equals(p);
            return false;
        }

        public override int GetHashCode()
        {
            var me = GetReadonly(this);
            if (ReferenceEquals(me, this))
                return base.GetHashCode();
            return me.GetHashCode();
        }

        private static NDReadOnlyProperty<TValue, TType> GetReadonly(NDReadOnlyProperty<TValue, TType> r)
        {
            if (r is INDProperty<TValue, TType> p)
                return p.ReadOnlyProperty;
            return r;
        }

        object IInternalNDReadOnlyProperty.GetValue(object obj)
        {
            if (obj is TType t)
                return PropertyRegistar.GetValue(this, t);
            throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }
        object IInternalNDReadOnlyProperty.GetLocalValue(object obj)
        {
            if (obj is TType t)
                return PropertyRegistar.GetValue(this, t);
            throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }

        bool IInternalNDReadOnlyProperty.HasLocalValue(object obj)
        {
            if (obj is TType t)
                return PropertyRegistar.HasLocalValue(this, t);
            throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }

        void IInternalNDReadOnlyProperty.CallChangeHandler(object objectToChange, object sender, object oldValue, object newValue)
        {
            if (!(objectToChange is TType))
                throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}", nameof(objectToChange));
            TValue nv;
            TValue ov;
            if (newValue != null)
            {
                if (newValue is TValue)
                    nv = (TValue)newValue;
                else
                    throw new ArgumentException($"Parameter was not of Type {typeof(TValue).FullName}", nameof(newValue));
            }
            else
                nv = (TValue)newValue;

            if (oldValue != null)
            {
                if (oldValue is TValue)
                    ov = (TValue)oldValue;
                else
                    throw new ArgumentException($"Parameter was not of Type {typeof(TValue).FullName}", nameof(oldValue));
            }
            else
                ov = (TValue)oldValue;


            if (objectToChange is TType t)
                PropertyRegistar.FireValueChanged(this, t, sender, ov, nv);


        }
    }


}
