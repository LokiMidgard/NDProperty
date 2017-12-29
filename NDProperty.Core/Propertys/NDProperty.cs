using System;
using NDProperty.Providers;

namespace NDProperty.Propertys
{
    /// <summary>
    /// This key alows read and write access to an NDProperty.
    /// </summary>
    /// <typeparam name="TValue">The type of the Property</typeparam>
    /// <typeparam name="TType">The type of the Object that defines the Property.</typeparam>
    public class NDPropertyKey<TKey, TValue, TType> : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>, IInternalNDProperty<TKey>
        where TType : class
    {
        internal readonly Func<TType, OnChanging<TKey, TValue>> changedMethod;

        /// <summary>
        /// Access the readonly property of this Property.
        /// </summary>
        /// <remarks>
        /// This Proeprty can be used to allow read but not write to an Property.
        /// </remarks>
        public NDReadOnlyPropertyKey<TKey, TValue, TType> ReadOnlyProperty { get; }

        internal NDPropertyKey(Func<TType, OnChanging<TKey, TValue>> changedMethod, TValue defaultValue, NDPropertySettings settigns) : base(defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TKey, TValue, TType>(defaultValue, settigns);
            this.changedMethod = changedMethod;
        }

        void IInternalNDProperty<TKey>.CallSetOmInHeritanceProvider(object obj, object source, object newValue, bool hasNewValue, object oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, object currentValue)
        {
            if (obj is TType t && source is TType sourceObject)
                InheritenceValueProvider<TKey>.Instance.SetValue(t, this, sourceObject, (TValue)newValue, hasNewValue, (TValue)oldValue, hasOldValue, currentProvider, (TValue)currentValue);
            else
                throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }
    }


}
