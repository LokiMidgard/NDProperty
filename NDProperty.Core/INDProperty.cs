namespace NDProperty
{
    public interface INDProperty<TValue, TType> where TType : class
    {
        NDReadOnlyProperty<TValue, TType> ReadOnlyProperty { get; }
    }
}