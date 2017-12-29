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

        protected bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue newValue, bool hasNewValue, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var (currentValue, currentProvider) = PropertyRegistar<TKey>.GetValueAndProvider(property, targetObject);
            var (oldValue, hasOldValue) = GetValue(targetObject, property);

            return Update(sender, targetObject, property, newValue, hasNewValue, updateCode, oldValue, hasOldValue, currentProvider, currentValue);
        }

        protected bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue newValue, bool hasNewValue, TValue oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, TValue currentValue)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            return Update(sender, targetObject, property, newValue, hasNewValue, () => true, oldValue, hasOldValue, currentProvider, currentValue);
        }

        internal bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue newValue, bool hasNewValue, Func<bool> updateCode, TValue oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, TValue currentValue)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var otherProviderIndex = PropertyRegistar<TKey>.ProviderOrder[currentProvider];
            var thisIndex = PropertyRegistar<TKey>.ProviderOrder[this];
            if (!property.Settigns.HasFlag(NDPropertySettings.CallOnChangedHandlerOnEquals) && Object.Equals(oldValue, newValue))
                return true;
            // We need to call the actial update after we recived the current old value. Otherwise we could already read the 
            OnChangingArg<TKey, TValue> onChangingArg;
            if (property as object is NDAttachedPropertyKey<TKey, TValue, TType> attach)
            {
                var attachArg = OnChangingArg.Create(targetObject, oldValue, hasOldValue, newValue, hasNewValue, this, currentProvider, currentValue, otherProviderIndex >= thisIndex, hasNewValue || !this.canDeletionBePrevented);
                onChangingArg = attachArg;
                attach.changedMethod(attachArg);
            }
            else if (property as object is NDPropertyKey<TKey, TValue, TType> p)
            {
                onChangingArg = OnChangingArg.Create(oldValue, hasOldValue, newValue, hasNewValue, this, currentProvider, currentValue, otherProviderIndex >= thisIndex, hasNewValue || !this.canDeletionBePrevented);
                p.changedMethod(targetObject)(onChangingArg);
            }
            else
                throw new NotSupportedException();
            var result = PropertyRegistar<TKey>.ChangeValue(sender, property, targetObject, onChangingArg, updateCode);
            FireEventHandler(property, targetObject, sender, new ChangedEventArgs<TValue, TType>(targetObject, oldValue, newValue));
            return result;
        }


        public abstract (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TKey, TValue, TType> property) where TType : class;

        public virtual void HigherProviderUpdated<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, ValueProvider<TKey> updatedProvider)
                    where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        { }
        public virtual void LowerProviderUpdated<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, ValueProvider<TKey> updatedProvider)
                    where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        { }

        private readonly ConditionalWeakTable<object, Dictionary<IInternalNDReadOnlyProperty<TKey>, EventWrapper>> listener = new ConditionalWeakTable<object, Dictionary<IInternalNDReadOnlyProperty<TKey>, EventWrapper>>();
        private readonly bool canDeletionBePrevented;

        private class EventWrapper { }
        private class EventWrapper<T> : EventWrapper
        {
            public event EventHandler<T> Handler;

            public int Count => Handler.GetInvocationList().Length;

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
        public void AddEventHandler<TValue, TType>(NDReadOnlyPropertyKey<TKey, TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            var dic = this.listener.GetOrCreateValue(obj);

            if (!dic.ContainsKey(property))
                dic.Add(property, new EventWrapper<ChangedEventArgs<TValue, TType>>());

            var wrapper = dic[property] as EventWrapper<ChangedEventArgs<TValue, TType>>;
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
        public void RemoveEventHandler<TValue, TType>(NDReadOnlyPropertyKey<TKey, TValue, TType> property, TType obj, EventHandler<ChangedEventArgs<TValue, TType>> handler) where TType : class
        {
            var dic = this.listener.GetOrCreateValue(obj);

            if (!dic.ContainsKey(property))
                return;

            var wrapper = dic[property] as EventWrapper<ChangedEventArgs<TValue, TType>>;
            wrapper.Handler -= handler;

            if (wrapper.Count == 0)
                dic.Remove(property);
        }


        private void FireEventHandler<TValue, TType>(NDReadOnlyPropertyKey<TKey, TValue, TType> property, TType obj, object sender, ChangedEventArgs<TValue, TType> args) where TType : class
        {
            var dic = this.listener.GetOrCreateValue(obj);

            if (!dic.ContainsKey(property))
                return;

            var wrapper = dic[property] as EventWrapper<ChangedEventArgs<TValue, TType>>;
            wrapper.Fire(sender, args);
        }



        //private class MyClass<TValue, TType, TPropertyType>
        //  where TType : class
        //    where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        //{

        //    public event ValueProviderUpdated<TKey, TValue, TType, TPropertyType> OnValueProviderUpdated;
        //}


        //public abstract bool HasValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TValue, TType> property) where TType : class;
    }

    //public delegate void ValueProviderUpdated<TKey, TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue newValue, TValue oldValue, ValueProvider<TKey> oldProvider)
    //      where TType : class
    //        where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>;

}
