using NDProperty.Propertys;


namespace NDProperty.Providers
{
    public sealed class InheritenceValueProvider<TKey> : ValueProvider<TKey>
    {
        private InheritenceValueProvider()
        {

        }
        public static InheritenceValueProvider<TKey> Instance { get; } = new InheritenceValueProvider<TKey>();
        public override (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, NDReadOnlyPropertyKey<TKey,TValue, TType> property)
        {
            if (property.Inherited)
            {
                // go up the tree
                var tree = PropertyRegistar<TKey>.Tree.GetTree(targetObject);
                while (tree.Parent != null)
                {
                    tree = tree.Parent;
                    if (tree.Current is TType instance)
                        return (PropertyRegistar<TKey>.GetValue(property, instance), true);
                }
            }
            return (default(TValue), false);

        }

        internal void SetValue<TValue, TType, TPropertyType>(TType targetObject, TPropertyType property, TValue newValue, TValue oldValue, ValueProvider<TKey> oldProvider, object sender = null)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            if (sender == null)
                sender = targetObject;
            Update(sender, targetObject, property, newValue, () => true, oldValue, oldProvider);
        }
    }

    //public abstract class ValueProviderManager
    //{
    //    private readonly Dictionary<object, object> providers = new Dictionary<object, object>();

    //    internal ValueProvider<TValue> GetProvider<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TValue, TType> property)
    //        where TType : class
    //    {

    //    }

    //    internal void SetProvider<TValue, TType>(ValueProvider<TValue> provider, TType targetObject, NDReadOnlyPropertyKey<TValue, TType> property) where TType : class
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
