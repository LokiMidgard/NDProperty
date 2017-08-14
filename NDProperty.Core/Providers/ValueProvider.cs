using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NDProperty.Propertys;


namespace NDProperty.Providers
{

    public abstract class ValueManager<TKey>
    {
        protected bool Update<TValue, TType, TPropertyType>(TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var (oldValue, otherProvider) = PropertyRegistar<TKey>.GetValueAndProvider(property, targetObject);

            return Update(targetObject, targetObject, property, value, updateCode, oldValue, otherProvider);
        }

        internal bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode, TValue oldValue, ValueManager<TKey> otherProvider)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey,TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var otherProviderIndex = PropertyRegistar<TKey>.ManagerOrder[otherProvider];
            var thisIndex = PropertyRegistar<TKey>.ManagerOrder[this];

            // We need to call the actial update after we recived the current old value. Otherwise we could already read the 
            OnChangingArg<TKey, TValue> onChangingArg;
            if (property as object is NDAttachedPropertyKey<TKey, TValue, TType> attach)
            {
                var attachArg = OnChangingArg.Create(targetObject, oldValue, value, this, otherProviderIndex >= thisIndex); ;
                onChangingArg = attachArg;
                attach.changedMethod(attachArg);
            }
            else if (property as object is NDPropertyKey<TKey, TValue, TType> p)
            {
                onChangingArg = OnChangingArg.Create(oldValue, value, this, otherProviderIndex >= thisIndex);
                p.changedMethod(targetObject)(onChangingArg);
            }
            else
                throw new NotSupportedException();
            return PropertyRegistar<TKey>.ChangeValue(sender, property, targetObject, onChangingArg, updateCode);
        }

        public abstract (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TKey, TValue, TType> property) where TType : class;
        //public abstract bool HasValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TValue, TType> property) where TType : class;
    }

    public sealed class DefaultValueManager<TKey> : ValueManager<TKey>
    {
        private DefaultValueManager()
        {

        }
        public static DefaultValueManager<TKey> Instance { get; } = new DefaultValueManager<TKey>();
        public override (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, NDReadOnlyPropertyKey<TKey, TValue, TType> property) => (property.DefaultValue, true);
    }

    public sealed class InheritenceValueManager<TKey> : ValueManager<TKey>
    {
        private InheritenceValueManager()
        {

        }
        public static InheritenceValueManager<TKey> Instance { get; } = new InheritenceValueManager<TKey>();
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

        internal void SetValue<TValue, TType, TPropertyType>(TType targetObject, TPropertyType property, TValue newValue, TValue oldValue, ValueManager<TKey> oldProvider, object sender = null)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            if (sender == null)
                sender = targetObject;
            Update(sender, targetObject, property, newValue, () => true, oldValue, oldProvider);
        }
    }

    public sealed class LocalValueManager<TKey> : ValueManager<TKey>
    {
        public static LocalValueManager<TKey> Instance { get; } = new LocalValueManager<TKey>();

        private LocalValueManager()
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
            return this.Update(changingObject, property, value, () =>
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
            return this.Update(changingObject, property, value, () =>
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
