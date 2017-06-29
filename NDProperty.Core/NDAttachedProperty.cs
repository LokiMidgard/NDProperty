namespace NDProperty
{
    public class NDAttachedPropertyKey<TValue, TType> : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly OnChanged<TValue, TType> changedMethod;

        public NDReadOnlyPropertyKey<TValue, TType> ReadOnlyProperty { get; }

        internal NDAttachedPropertyKey(OnChanged<TValue, TType> changedMethod,  TValue defaultValue, NDPropertySettings settigns) : base(defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TValue, TType>( defaultValue, settigns);
            this.changedMethod = changedMethod;
        }

    }


}
