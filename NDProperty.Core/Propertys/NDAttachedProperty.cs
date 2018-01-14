using System;
using NDProperty.Providers;

namespace NDProperty.Propertys
{
    /// <summary>
    /// The key to allow read and write access to an attached property.
    /// </summary>
    /// <typeparam name="TValue">The type of the Property</typeparam>
    /// <typeparam name="TType">The type of the Object that this property can attached to.</typeparam>
    public class NDAttachedPropertyKey<TKey, TType, TValue> : NDBasePropertyKey<TKey, TType, TValue>, IInternalNDProperty<TKey>
        where TType : class
    {
        internal readonly OnChanging<TKey, TType, TValue> changedMethod;


        internal NDAttachedPropertyKey(OnChanging<TKey, TType, TValue> changedMethod, TValue defaultValue, NDPropertySettings settigns) : base(defaultValue, settigns)
        {
            this.changedMethod = changedMethod;
        }

        void IInternalNDProperty<TKey>.CallSetOmInHeritanceProvider(object obj, object source, object value, bool hasNewValue, object oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, object currentValue)
        {
            if (obj is TType t && source is TType sourceObject)
                InheritenceValueProvider<TKey>.Instance.SetValue(t, this, sourceObject, (TValue)value, hasNewValue, (TValue)oldValue, hasOldValue, currentProvider, (TValue)currentValue);
            else
                throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }
    }


}
