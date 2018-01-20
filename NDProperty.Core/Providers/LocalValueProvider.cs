using System;
using NDProperty.Propertys;


namespace NDProperty.Providers
{
    public sealed class LocalValueProvider<TKey> : ValueProvider<TKey>
    {
        public static LocalValueProvider<TKey> Instance { get; } = new LocalValueProvider<TKey>();

        private LocalValueProvider() : base(false)
        {

        }


        public bool SetValue<TType, TValue>(NDBasePropertyKey<TKey, TType, TValue> property, TType changingObject, TValue value)
            where TType : class
        {
            //if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals))
            //{
            //    var oldValue = PropertyRegistar<TKey>.GetValue(property, changingObject);
            //    if (Object.Equals(oldValue, value))
            //        return true;
            //}

            var hasValue = value != null || property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull);

            return this.Update(changingObject, changingObject, property, value, hasValue, () =>
            {
                if (value == null && !property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull))
                    PropertyRegistar<TKey>.Lookup<TType, TValue>.Property.Remove((changingObject, property));
                else
                    PropertyRegistar<TKey>.Lookup<TType, TValue>.Property[(changingObject, property)] = value;

                return true;
            });

        }
        //public bool SetValue<TType, TValue>(NDAttachedPropertyKey<TKey, TType, TValue> property, TType changingObject, TValue value) where TType : class
        //{
        //    if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals))
        //    {
        //        var oldValue = PropertyRegistar<TKey>.GetValue(property, changingObject);
        //        if (Object.Equals(oldValue, value))
        //            return true;
        //    }
        //    return this.Update(changingObject, changingObject, property, value, () =>
        //    {
        //        if (value == null && !property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull))
        //            PropertyRegistar<TKey>.Lookup<TType, TValue>.Property.Remove((changingObject, property));
        //        else
        //            PropertyRegistar<TKey>.Lookup<TType, TValue>.Property[(changingObject, property)] = value;

        //        return true;
        //    });
        //}

        public override (TValue value, bool hasValue) GetValue<TType, TValue>(TType targetObject, NDReadOnlyPropertyKey<TKey, TType, TValue> property)
        {
            var hasValue = PropertyRegistar<TKey>.Lookup<TType, TValue>.Property.ContainsKey((targetObject, property));

            if (hasValue)
                return (PropertyRegistar<TKey>.Lookup<TType, TValue>.Property[(targetObject, property)], hasValue);
            return (default(TValue), hasValue);
        }



        public bool RemoveValue<TType, TValue>(NDBasePropertyKey<TKey, TType, TValue> property, TType changingObject)
              where TType : class
        {
            return this.Update(changingObject, changingObject, property, default(TValue), false, () =>
            {
                PropertyRegistar<TKey>.Lookup<TType, TValue>.Property.Remove((changingObject, property));
                return true;
            });
        }
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
