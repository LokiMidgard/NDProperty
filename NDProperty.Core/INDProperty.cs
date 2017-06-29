namespace NDProperty
{
    public interface INDProperty<TValue, TType> where TType : class
    {
        NDReadOnlyPropertyKey<TValue, TType> ReadOnlyProperty { get; }
    }
}