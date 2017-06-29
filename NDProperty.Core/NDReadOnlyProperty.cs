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
        /// <summary>
        /// Will define that this Property defines the Parent - Child relationship.
        /// </summary>
        ParentReference = 1 << 2,
        /// <summary>
        /// Will define that this Property should be read only. 
        /// </summary>
        /// <remarks>
        /// This property will be used for code generation in the first place. <para/>
        /// It will not automaticly result in a readonly behavior. You need to restrict
        /// access to the <see cref="NDPropertyKey{TValue, TType}"/> and only hand out the <see cref="NDReadOnlyPropertyKey{TValue, TType}"/>.
        /// </remarks>
        ReadOnly = 1 << 3,
        /// <summary>
        /// Defines that this Property inherits its value fromits parent if no local value was set.
        /// </summary>
        /// <remarks>
        /// This requires that a Parent Reference exists on this object. Otherwise it does nothing.
        /// </remarks>
        Inherited = 1 << 4,
        /// <summary>
        /// Using this setting allows setting the local value to <c>null</c>.
        /// </summary>
        /// <remarks>
        /// If this setting is not set, assigning <c>null</c> to an Property will remove its value, setting an inherited value or the specified default value.
        /// If this setting is set assigning <c>null</c> to this Property will assigne this value to the property overiting iheritance and specified default value.
        /// </remarks>
        SetLocalExplicityNull = 1 << 5,


    }
    public class NDReadOnlyPropertyKey<TValue, TType> : IInternalNDReadOnlyProperty where TType : class
    {
        public bool Inherited => Settigns.HasFlag(NDPropertySettings.Inherited);
        public NDPropertySettings Settigns { get; }

        public TValue DefaultValue { get; }

        internal NDReadOnlyPropertyKey(TValue defaultValue, NDPropertySettings settigns)
        {
            Settigns = settigns;
            DefaultValue = defaultValue;
        }

        public bool Equals(NDReadOnlyPropertyKey<TValue, TType> obj)
        {
            var other = GetReadonly(obj);
            var me = GetReadonly(this);
            if (ReferenceEquals(me, this) && ReferenceEquals(obj, other))
                return ReferenceEquals(me, other);
            return me.Equals(other);
        }
        public override bool Equals(object obj)
        {
            if (obj is NDReadOnlyPropertyKey<TValue, TType> p)
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

        private static NDReadOnlyPropertyKey<TValue, TType> GetReadonly(NDReadOnlyPropertyKey<TValue, TType> r)
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
