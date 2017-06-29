using System;

namespace NDProperty.Propertys
{

    /// <summary>
    /// This key allows read access to a NDProperty
    /// </summary>
    /// <typeparam name="TValue">The type of the Property</typeparam>
    /// <typeparam name="TType">The type of the Object that defines the Property.</typeparam>
    public class NDReadOnlyPropertyKey<TValue, TType> : IInternalNDReadOnlyProperty where TType : class
    {
        /// <summary>
        /// Returns if this Propety is inherited.
        /// </summary>
        public bool Inherited => Settigns.HasFlag(NDPropertySettings.Inherited);
        /// <summary>
        /// The settings of this Property.
        /// </summary>
        public NDPropertySettings Settigns { get; }

        /// <summary>
        /// The default value that this property has if no value is set.
        /// </summary>
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
