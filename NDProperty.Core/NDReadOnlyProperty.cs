﻿using System;

namespace NDProperty
{
    public class NDReadOnlyProperty<TValue, TType> : IInternalNDReadOnlyProperty where TType : class
    {
        public bool Inherited { get; }

        public NullTreatment NullTreatment { get; }

        internal NDReadOnlyProperty(bool inherited, NullTreatment nullTreatment)
        {

            Inherited = inherited;
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