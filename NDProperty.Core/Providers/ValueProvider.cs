using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NDProperty.Propertys;


namespace NDProperty.Providers
{
    /// <summary>
    /// Base class to provide aloternative value lookup for NDP Propertys
    /// </summary>
    /// <typeparam name="TKey">The Configuration</typeparam>
    public abstract class ValueProvider<TKey>
    {

        protected ValueProvider(bool canDeletionBePrevented)
        {
            this.canDeletionBePrevented = canDeletionBePrevented;
        }

        protected bool Update<TType, TValue>(object sender, TType targetObject, NDBasePropertyKey<TKey, TType, TValue> property, TValue newValue, bool hasNewValue, Func<bool> updateCode)
            where TType : class
        {
            var (currentValue, currentProvider) = PropertyRegistar<TKey>.GetValueAndProvider(property, targetObject);
            var (oldValue, hasOldValue) = GetValue(targetObject, property);

            return Update(sender, targetObject, property, newValue, hasNewValue, updateCode, oldValue, hasOldValue, currentProvider, currentValue);
        }

        protected bool Update<TType, TValue>(object sender, TType targetObject, NDBasePropertyKey<TKey, TType, TValue> property, TValue newValue, bool hasNewValue, TValue oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, TValue currentValue)
            where TType : class
        {
            return Update(sender, targetObject, property, newValue, hasNewValue, () => true, oldValue, hasOldValue, currentProvider, currentValue);
        }

        internal bool Update<TType, TValue>(object sender, TType targetObject, NDBasePropertyKey<TKey, TType, TValue> property, TValue newProviderValue, bool hasNewValue, Func<bool> updateCode, TValue oldProviderValue, bool hasOldValue, ValueProvider<TKey> oldActualProvider, TValue oldActualValue)
            where TType : class
        {
            var otherProviderIndex = PropertyRegistar<TKey>.ProviderOrder[oldActualProvider];
            var thisIndex = PropertyRegistar<TKey>.ProviderOrder[this];
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals) && Object.Equals(oldProviderValue, newProviderValue) && hasOldValue == hasNewValue)
                return true;

            TValue newActualValue = default;
            ValueProvider<TKey> newActualProvider = null;
            if (this == oldActualProvider && !hasNewValue)
            {
                // the current value was provided by the changing provider but now it will no longer have a value
                // we need to find out what the new value will be.
                bool found = false;
                foreach (var item in PropertyRegistar<TKey>.ValueProviders)
                {
                    if (item == this)
                        continue;
                    var (providerValue, hasValue) = item.GetValue(targetObject, property);
                    if (hasValue)
                    {
                        found = true;
                        newActualProvider = item;
                        newActualValue = providerValue;
                        break;
                    }
                }
                if (!found)
                    throw new InvalidOperationException("No Value Found");
            }
            else if (otherProviderIndex >= thisIndex)
            {
                newActualProvider = this;
                newActualValue = newProviderValue;
            }
            else
            {
                newActualProvider = oldActualProvider;
                newActualValue = oldActualValue;
            }


            // We need to call the actial update after we recived the current old value. Otherwise we could already read the 
            OnChangingArg<TKey, TValue> onChangingArg;
            if (property as object is NDAttachedPropertyKey<TKey, TType, TValue> attach)
            {
                var attachArg = OnChangingArg.Create(targetObject, oldProviderValue, hasOldValue, newProviderValue, hasNewValue, this, oldActualProvider, newActualProvider, oldActualValue, newActualValue, hasNewValue || !this.canDeletionBePrevented);
                onChangingArg = attachArg;
                attach.changedMethod(attachArg);
            }
            else if (property as object is NDPropertyKey<TKey, TType, TValue> p)
            {
                onChangingArg = OnChangingArg.Create(oldProviderValue, hasOldValue, newProviderValue, hasNewValue, this, oldActualProvider, newActualProvider, oldActualValue, newActualValue, hasNewValue || !this.canDeletionBePrevented);
                p.changedMethod(targetObject)(onChangingArg);
            }
            else
                throw new NotSupportedException();
            var result = PropertyRegistar<TKey>.ChangeValue(sender, property, targetObject, onChangingArg, updateCode);
            FireEventHandler(property, targetObject, sender, ChangedEventArgs.Create(targetObject, property, oldProviderValue, newProviderValue));
            return result;
        }


        public abstract (TValue value, bool hasValue) GetValue<TType, TValue>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TKey, TType, TValue> property) where TType : class;

        public virtual void HigherProviderUpdated<TType, TValue>(object sender, TType targetObject, NDBasePropertyKey<TKey, TType, TValue> property, TValue value, ValueProvider<TKey> updatedProvider)
                    where TType : class
        { }
        public virtual void LowerProviderUpdated<TType, TValue>(object sender, TType targetObject, NDBasePropertyKey<TKey, TType, TValue> property, TValue value, ValueProvider<TKey> updatedProvider)
                    where TType : class
        { }

        private readonly ConditionalWeakTable<object, Dictionary<IInternalNDReadOnlyProperty<TKey>, EventWrapper>> listener = new ConditionalWeakTable<object, Dictionary<IInternalNDReadOnlyProperty<TKey>, EventWrapper>>();
        private readonly bool canDeletionBePrevented;

        private class EventWrapper { }
        private class EventWrapper<T> : EventWrapper
        {
            public event EventHandler<T> Handler;

            public int Count => Handler?.GetInvocationList()?.Length ?? 0;

            public void Fire(object sender, T arg) => Handler?.Invoke(sender, arg);
        }

        /// <summary>
        /// Adds an event handler to the specific Property on the Specific object.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property whichthe Eventhandler will be watch.</param>
        /// <param name="obj">The object the Eventhandler will be watch.</param>
        /// <param name="handler">The handler that will be called if the Property changes.</param>
        public void AddEventHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj, EventHandler<ChangedEventArgs<TKey, TType, TValue>> handler) where TType : class
        {
            var dic = this.listener.GetOrCreateValue(obj);

            if (!dic.ContainsKey(property))
                dic.Add(property, new EventWrapper<ChangedEventArgs<TKey, TType, TValue>>());

            var wrapper = dic[property] as EventWrapper<ChangedEventArgs<TKey, TType, TValue>>;
            wrapper.Handler += handler;
        }
        /// <summary>
        /// Removes an event handler from the specific Property on the Specific object.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TType">The Type of the Object that defines the Property</typeparam>
        /// <param name="property">The Property whichthe Eventhandler will be removed.</param>
        /// <param name="obj">The object the Eventhandler will be removed.</param>
        /// <param name="handler">The handler that should no longer be called if the Property changes.</param>
        public void RemoveEventHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj, EventHandler<ChangedEventArgs<TKey, TType, TValue>> handler) where TType : class
        {
            var dic = this.listener.GetOrCreateValue(obj);

            if (!dic.ContainsKey(property))
                return;

            var wrapper = dic[property] as EventWrapper<ChangedEventArgs<TKey, TType, TValue>>;
            wrapper.Handler -= handler;

            if (wrapper.Count == 0)
                dic.Remove(property);
        }


        private void FireEventHandler<TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType obj, object sender, ChangedEventArgs<TKey, TType, TValue> args) where TType : class
        {
            var dic = this.listener.GetOrCreateValue(obj);

            if (!dic.ContainsKey(property))
                return;

            var wrapper = dic[property] as EventWrapper<ChangedEventArgs<TKey, TType, TValue>>;
            wrapper.Fire(sender, args);
        }



        //private class MyClass<TType, TValue, TPropertyType>
        //  where TType : class
        //    where TPropertyType : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>
        //{

        //    public event ValueProviderUpdated<TKey, TType, TValue, TPropertyType> OnValueProviderUpdated;
        //}


        //public abstract bool HasValue<TType, TValue>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TType, TValue> property) where TType : class;
    }

    //public delegate void ValueProviderUpdated<TKey, TType, TValue, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue newValue, TValue oldValue, ValueProvider<TKey> oldProvider)
    //      where TType : class
    //        where TPropertyType : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>;

}
