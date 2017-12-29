using System;
using NDProperty.Providers;

namespace NDProperty.Propertys
{
    /// <summary>
    /// The key to allow read and write access to an attached property.
    /// </summary>
    /// <typeparam name="TValue">The type of the Property</typeparam>
    /// <typeparam name="TType">The type of the Object that this property can attached to.</typeparam>
    public class NDAttachedPropertyKey<TKey, TValue, TType> : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType> , IInternalNDProperty<TKey>
        where TType : class
    {
        internal readonly OnChanging<TKey, TValue, TType> changedMethod;

        /// <summary>
        /// Access the readonly property of this Property.
        /// </summary>
        /// <remarks>
        /// This Proeprty can be used to allow read but not write to an Property.
        /// </remarks>
        public NDReadOnlyPropertyKey<TKey, TValue, TType> ReadOnlyProperty { get; }

        internal NDAttachedPropertyKey(OnChanging<TKey, TValue, TType> changedMethod,  TValue defaultValue, NDPropertySettings settigns) : base(defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TKey, TValue, TType>( defaultValue, settigns);
            this.changedMethod = changedMethod;
        }

        void IInternalNDProperty<TKey>.CallSetOmInHeritanceProvider(object obj, object source, object value, bool hasNewValue, object oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, object currentValue)
        {
            if (obj is TType t && source is TType sourceObject && value is TValue newValue && oldValue is TValue oldValue2 && currentValue is TValue currentValue2)
                InheritenceValueProvider<TKey>.Instance.SetValue(t, this, sourceObject, newValue, hasNewValue, oldValue2, hasOldValue, currentProvider, currentValue2);
            else
                throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }
    }


}
