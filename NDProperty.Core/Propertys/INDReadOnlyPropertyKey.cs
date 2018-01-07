namespace NDProperty.Propertys
{
    public interface INDReadOnlyPropertyKey<TKey,   in TType, TValue> where TType : class
    {
        TValue DefaultValue { get; }
        bool Inherited { get; }
        NDPropertySettings Settigns { get; }
    }
}