namespace NDProperty
{
    /// <summary>
    /// The key to allow read and write access to an attached property.
    /// </summary>
    /// <typeparam name="TValue">The type of the Property</typeparam>
    /// <typeparam name="TType">The type of the Object that this property can attached to.</typeparam>
    public class NDAttachedPropertyKey<TValue, TType> : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly OnChanged<TValue, TType> changedMethod;

        /// <summary>
        /// Access the readonly property of this Property.
        /// </summary>
        /// <remarks>
        /// This Proeprty can be used to allow read but not write to an Property.
        /// </remarks>
        public NDReadOnlyPropertyKey<TValue, TType> ReadOnlyProperty { get; }

        internal NDAttachedPropertyKey(OnChanged<TValue, TType> changedMethod,  TValue defaultValue, NDPropertySettings settigns) : base(defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TValue, TType>( defaultValue, settigns);
            this.changedMethod = changedMethod;
        }

    }


}
