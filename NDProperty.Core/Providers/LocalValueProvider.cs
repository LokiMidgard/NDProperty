using System;
using NDProperty.Propertys;


namespace NDProperty.Providers
{
    public sealed class LocalValueProvider<TKey> : ValueProvider<TKey>
    {
        public static LocalValueProvider<TKey> Instance { get; } = new LocalValueProvider<TKey>();

        private LocalValueProvider()
        {

        }


        public bool SetValue<TValue, TType>(NDPropertyKey<TKey, TValue, TType> property, TType changingObject, TValue value) where TType : class
        {
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals))
            {
                var oldValue = PropertyRegistar<TKey>.GetValue(property, changingObject);
                if (Object.Equals(oldValue, value))
                    return true;
            }
            return this.Update(changingObject, changingObject, property, value, () =>
            {
                if (value == null && !property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull))
                    PropertyRegistar<TKey>.Lookup<TValue, TType>.Property.Remove((changingObject, property));
                else
                    PropertyRegistar<TKey>.Lookup<TValue, TType>.Property[(changingObject, property)] = value;

                return true;
            });

        }
        public bool SetValue<TValue, TType>(NDAttachedPropertyKey<TKey, TValue, TType> property, TType changingObject, TValue value) where TType : class
        {
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals))
            {
                var oldValue = PropertyRegistar<TKey>.GetValue(property, changingObject);
                if (Object.Equals(oldValue, value))
                    return true;
            }
            return this.Update(changingObject, changingObject, property, value, () =>
            {
                if (value == null && !property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull))
                    PropertyRegistar<TKey>.Lookup<TValue, TType>.Property.Remove((changingObject, property));
                else
                    PropertyRegistar<TKey>.Lookup<TValue, TType>.Property[(changingObject, property)] = value;

                return true;
            });
        }

        public override (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, NDReadOnlyPropertyKey<TKey, TValue, TType> property)
        {
            var hasValue = PropertyRegistar<TKey>.Lookup<TValue, TType>.Property.ContainsKey((targetObject, property));

            if (hasValue)
                return (PropertyRegistar<TKey>.Lookup<TValue, TType>.Property[(targetObject, property)], hasValue);
            return (default(TValue), hasValue);
        }



        public bool RemoveValue<TValue, TType>(NDReadOnlyPropertyKey<TKey, TValue, TType> property, TType obj) where TType : class
        {
            return PropertyRegistar<TKey>.Lookup<TValue, TType>.Property.Remove((obj, property));
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
