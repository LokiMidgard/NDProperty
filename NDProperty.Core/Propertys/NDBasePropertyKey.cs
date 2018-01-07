namespace NDProperty.Propertys
{
    public abstract class NDBasePropertyKey<TKey, TType, TValue> : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>
        where TType : class
    {
        internal NDBasePropertyKey(TValue defaultValue, NDPropertySettings settigns) : base(defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TKey, TType, TValue>(defaultValue, settigns);
        }

        /// <summary>
        /// Access the readonly property of this Property.
        /// </summary>
        /// <remarks>
        /// This Proeprty can be used to allow read but not write to an Property.
        /// </remarks>
        public NDReadOnlyPropertyKey<TKey, TType, TValue> ReadOnlyProperty { get; }
    }
}