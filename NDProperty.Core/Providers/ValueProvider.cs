using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NDProperty.Propertys;


namespace NDProperty.Providers
{

    public abstract class ValueManager
    {
        protected bool Update<TValue, TType, TPropertyType>(TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType>
        {
            var (oldValue, otherProvider) = PropertyRegistar.GetValueAndProvider(property, targetObject);

            return Update(targetObject, property, value, updateCode, oldValue, otherProvider);
        }

        internal bool Update<TValue, TType, TPropertyType>(TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode, TValue oldValue, ValueManager otherProvider)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType>
        {
            var otherProviderIndex = PropertyRegistar.managerOrder[otherProvider];
            var thisIndex = PropertyRegistar.managerOrder[this];

            // We need to call the actial update after we recived the current old value. Otherwise we could already read the 
            OnChangingArg<TValue> onChangingArg;
            if (property as object is NDAttachedPropertyKey<TValue, TType> attach)
            {
                var attachArg = OnChangingArg.Create(targetObject, oldValue, value, this, otherProviderIndex > thisIndex); ;
                onChangingArg = attachArg;
                attach.changedMethod(attachArg);
            }
            else if (property as object is NDPropertyKey<TValue, TType> p)
            {
                onChangingArg = OnChangingArg.Create(oldValue, value, this, otherProviderIndex > thisIndex);
                p.changedMethod(targetObject)(onChangingArg);
            }
            else
                throw new NotSupportedException();
            return PropertyRegistar.ChangeValue(property, targetObject, onChangingArg, updateCode);
        }

        public abstract (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TValue, TType> property) where TType : class;
        //public abstract bool HasValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TValue, TType> property) where TType : class;
    }

    public sealed class DefaultValueManager : ValueManager
    {
        private DefaultValueManager()
        {

        }
        public static DefaultValueManager Instance { get; } = new DefaultValueManager();
        public override (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, NDReadOnlyPropertyKey<TValue, TType> property) => (property.DefaultValue, true);
    }

    public sealed class InheritenceValueManager : ValueManager
    {
        private InheritenceValueManager()
        {

        }
        public static InheritenceValueManager Instance { get; } = new InheritenceValueManager();
        public override (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, NDReadOnlyPropertyKey<TValue, TType> property)
        {
            if (property.Inherited)
            {
                // go up the tree
                var tree = PropertyRegistar.Tree.GetTree(targetObject);
                while (tree.Parent != null)
                {
                    tree = tree.Parent;
                    if (tree.Current is TType instance)
                        return (PropertyRegistar.GetValue(property, instance), true);
                }
            }
            return (default(TValue), false);

        }

        internal void SetValue<TValue, TType, TPropertyType>(TType targetObject, TPropertyType property, TValue newValue, TValue oldValue, ValueManager oldProvider, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType>
        {
            Update(targetObject, property, newValue, updateCode, oldValue, oldProvider);
        }
    }

    public sealed class LocalValueManager : ValueManager
    {
        public static LocalValueManager Instance { get; } = new LocalValueManager();

        private LocalValueManager()
        {

        }


        public bool SetValue<TValue, TType>(NDPropertyKey<TValue, TType> property, TType changingObject, TValue value) where TType : class
        {
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals))
            {
                var oldValue = PropertyRegistar.GetValue(property, changingObject);
                if (Object.Equals(oldValue, value))
                    return true;
            }
            return this.Update(changingObject, property, value, () =>
            {
                if (value == null && !property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull))
                    PropertyRegistar.Lookup<TValue, TType>.Property.Remove((changingObject, property));
                else
                    PropertyRegistar.Lookup<TValue, TType>.Property[(changingObject, property)] = value;

                return true;
            });

        }
        public bool SetValue<TValue, TType>(NDAttachedPropertyKey<TValue, TType> property, TType changingObject, TValue value) where TType : class
        {
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals))
            {
                var oldValue = PropertyRegistar.GetValue(property, changingObject);
                if (Object.Equals(oldValue, value))
                    return true;
            }
            return this.Update(changingObject, property, value, () =>
            {
                if (value == null && !property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull))
                    PropertyRegistar.Lookup<TValue, TType>.Property.Remove((changingObject, property));
                else
                    PropertyRegistar.Lookup<TValue, TType>.Property[(changingObject, property)] = value;

                return true;
            });
        }

        public override (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, NDReadOnlyPropertyKey<TValue, TType> property)
        {
            var hasValue = PropertyRegistar.Lookup<TValue, TType>.Property.ContainsKey((targetObject, property));

            if (hasValue)
                return (PropertyRegistar.Lookup<TValue, TType>.Property[(targetObject, property)], hasValue);
            return (default(TValue), hasValue);
        }



        public bool RemoveValue<TValue, TType>(NDReadOnlyPropertyKey<TValue, TType> property, TType obj) where TType : class
        {
            return PropertyRegistar.Lookup<TValue, TType>.Property.Remove((obj, property));
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
