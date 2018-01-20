using System;
using System.Collections.Generic;
using System.Reflection;
using NDProperty.Propertys;


namespace NDProperty.Providers
{
    public sealed class InheritenceValueProvider<TKey> : ValueProvider<TKey>
    {
        private InheritenceValueProvider() : base(false)
        {

        }
        public static InheritenceValueProvider<TKey> Instance { get; } = new InheritenceValueProvider<TKey>();
        public override (TValue value, bool hasValue) GetValue<TType, TValue>(TType targetObject, NDReadOnlyPropertyKey<TKey, TType, TValue> property)
        {
            if (property.Inherited)
            {
                var dic = table.GetOrCreateValue(targetObject);
                if (dic.ContainsKey(property))
                    return ((TValue)dic[property].value, true);
            }
            return (default(TValue), false);

        }


        internal (object source, object value, ValueProvider<TKey> provider) SearchNewValue(object targetObject, IInternalNDReadOnlyProperty<TKey> property, Type definedType)
        {
            var tree = PropertyRegistar<TKey>.Tree.GetTree(targetObject);

            while (tree.Parent != null)
            {
                tree = tree.Parent;
                if (definedType.IsAssignableFrom(tree.Current.GetType()))
                {
                    var instance = tree.Current;
                    var (value, provider) = property.GetValueAndProvider(instance);
                    return (instance, value, provider);
                }
            }
            return (null, null, null);

        }
        internal bool IsParantChangeInteresting(object targetObject, IInternalNDReadOnlyProperty<TKey> property, Type definedType, object removedParent)
        {

            var dic = table.GetOrCreateValue(targetObject);
            if (dic.ContainsKey(property))
            {

                var (oldValue, oldSource) = dic[property];
                var tree = PropertyRegistar<TKey>.Tree.GetTree(targetObject);

                while (tree.Parent != null)
                {
                    tree = tree.Parent;
                    if (definedType.IsAssignableFrom(tree.Current.GetType()))
                    {
                        var instance = tree.Current;
                        if (instance == removedParent)
                            return true;
                        if (instance == oldSource)
                            return false;
                        //return (PropertyRegistar<TKey>.GetValue(property, instance), true);
                    }
                }
                return true;
            }
            else // previosly no parent
            {
                return true;
                //// go up the tree
                //var tree = PropertyRegistar<TKey>.Tree.GetTree(targetObject);
                //while (tree.Parent != null)
                //{
                //    tree = tree.Parent;
                //    if (tree.Current is TType instance)
                //    {

                //        return (PropertyRegistar<TKey>.GetValue(property, instance), true);
                //    }
                //}
            }
        }


        internal void SetValue<TType, TValue>(TType targetObject, NDBasePropertyKey<TKey, TType, TValue> property, TType sourceObject, TValue newValue, bool hasNewValue, TValue oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, TValue currentValue, object sender = null)
            where TType : class
        {
            if (sender == null)
                sender = targetObject;
            Update(sender, targetObject, property, newValue, hasNewValue, () =>
            {
                var dic = table.GetOrCreateValue(targetObject);
                if (hasNewValue)
                    dic[property] = (newValue, sourceObject);
                else
                {
                    dic.Remove(property);
                    if (dic.Count == 0)
                        table.Remove(targetObject);
                }
                return true;
            }, oldValue, hasOldValue, currentProvider, currentValue);
        }


        private System.Runtime.CompilerServices.ConditionalWeakTable<object, Dictionary<IInternalNDReadOnlyProperty<TKey>, (object value, object source)>> table = new System.Runtime.CompilerServices.ConditionalWeakTable<object, Dictionary<IInternalNDReadOnlyProperty<TKey>, (object value, object source)>>();
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
