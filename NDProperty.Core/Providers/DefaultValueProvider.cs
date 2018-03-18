using NDProperty.Propertys;


namespace NDProperty.Providers
{
    public sealed class DefaultValueProvider<TKey> : ValueProvider<TKey>
    {
        private DefaultValueProvider() : base(false, false, false)
        {

        }
        public static DefaultValueProvider<TKey> Instance { get; } = new DefaultValueProvider<TKey>();
        public override (TValue value, bool hasValue) GetValue<TType, TValue>(TType targetObject, NDReadOnlyPropertyKey<TKey, TType, TValue> property) => (property.DefaultValue, true);
    }
}
