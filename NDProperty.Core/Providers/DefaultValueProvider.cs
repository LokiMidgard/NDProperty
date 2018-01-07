using NDProperty.Propertys;


namespace NDProperty.Providers
{
    public sealed class DefaultValueProvider<TKey> : ValueProvider<TKey>
    {
        private DefaultValueProvider() : base(false)
        {

        }
        public static DefaultValueProvider<TKey> Instance { get; } = new DefaultValueProvider<TKey>();
        public override (TValue value, bool hasValue) GetValue<TType, TValue>(TType targetObject, NDReadOnlyPropertyKey<TKey, TType, TValue> property) => (property.DefaultValue, true);
    }

    //public abstract class ValueProviderManager
    //{
    //    private readonly Dictionary<object, object> providers = new Dictionary<object, object>();

    //    internal ValueProvider<TValue> GetProvider<TType, TValue>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TType, TValue> property)
    //        where TType : class
    //    {

    //    }

    //    internal void SetProvider<TType, TValue>(ValueProvider<TValue> provider, TType targetObject, NDReadOnlyPropertyKey<TType, TValue> property) where TType : class
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public abstract class ValueProvider<TValue>
    //{

    //    public ValueProviderManager Manager { get; }
    //    public bool HasValue { private set; get; }

    //    public TValue CurrentValue { get; private set; }

    //    internal event EventHandler<IValueWillChangeEventArgs<TValue>> ValueWillChange;

    //    protected ValueProvider(ValueProviderManager manager)
    //    {
    //        Manager = manager;
    //    }

    //    protected void UpdateValue(TValue value, bool hasValue)
    //    {
    //        var valueWillChangeEventArgs = new ValueWillChangeEventArgs<TValue>(value, hasValue);
    //        ValueWillChange?.Invoke(this, valueWillChangeEventArgs);
    //        CurrentValue = value;
    //        HasValue = hasValue;
    //        valueWillChangeEventArgs.FireEvents();
    //    }
    //}
    //internal class ValueWillChangeEventArgs<TValue> : EventArgs, IValueWillChangeEventArgs<TValue>
    //{

    //    public ValueWillChangeEventArgs(TValue value, bool hasValue)
    //    {
    //        NewValue = value;
    //        HasValue = HasValue;
    //    }

    //    public event Action AfterChange;

    //    public TValue NewValue { get; }
    //    public bool HasValue { get; }

    //    internal void FireEvents()
    //    {
    //        AfterChange?.Invoke();
    //    }
    //}

}
