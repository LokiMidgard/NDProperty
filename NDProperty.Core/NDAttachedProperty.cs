namespace NDProperty
{
    public class NDAttachedProperty<TValue, TType> : NDReadOnlyProperty<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly OnChanged<TValue, TType> changedMethod;

        public NDReadOnlyProperty<TValue, TType> ReadOnlyProperty { get; }

        internal NDAttachedProperty(OnChanged<TValue, TType> changedMethod,  NullTreatment nullTreatment, TValue defaultValue, NDPropertySettings settigns) : base(nullTreatment, defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyProperty<TValue, TType>( nullTreatment, defaultValue, settigns);
            this.changedMethod = changedMethod;
        }

    }


}
