namespace NDProperty
{
    public class NDAttachedPropertyKey<TValue, TType> : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly OnChanged<TValue, TType> changedMethod;

        public NDReadOnlyPropertyKey<TValue, TType> ReadOnlyProperty { get; }

        internal NDAttachedPropertyKey(OnChanged<TValue, TType> changedMethod,  NullTreatment nullTreatment, TValue defaultValue, NDPropertySettings settigns) : base(nullTreatment, defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TValue, TType>( nullTreatment, defaultValue, settigns);
            this.changedMethod = changedMethod;
        }

    }


}
