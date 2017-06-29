using System;

namespace NDProperty.Propertys
{
    /// <summary>
    /// This key alows read and write access to an NDProperty.
    /// </summary>
    /// <typeparam name="TValue">The type of the Property</typeparam>
    /// <typeparam name="TType">The type of the Object that defines the Property.</typeparam>
    public class NDPropertyKey<TValue, TType> : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly Func<TType, OnChanging<TValue>> changedMethod;

        /// <summary>
        /// Access the readonly property of this Property.
        /// </summary>
        /// <remarks>
        /// This Proeprty can be used to allow read but not write to an Property.
        /// </remarks>
        public NDReadOnlyPropertyKey<TValue, TType> ReadOnlyProperty { get; }

        internal NDPropertyKey(Func<TType, OnChanging<TValue>> changedMethod, TValue defaultValue, NDPropertySettings settigns) : base(defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TValue, TType>(defaultValue, settigns);
            this.changedMethod = changedMethod;
        }
    }


}
