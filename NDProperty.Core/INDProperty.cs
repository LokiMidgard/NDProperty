namespace NDProperty
{
    internal interface INDProperty<TValue, TType>
    {
        NDReadOnlyProperty<TValue, TType> ReadOnlyProperty { get; }
    }
}