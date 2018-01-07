using System;

namespace NDProperty.Propertys
{
    public static class ChangedEventArgs
    {
        public static ChangedEventArgs<TKey, TType, TValue> Create<TKey, TType, TValue>(TType objectThatChanged, NDReadOnlyPropertyKey<TKey, TType, TValue> changedProperty, TValue oldValue, TValue newValue) where TType : class => new ChangedEventArgs<TKey, TType, TValue>(objectThatChanged, changedProperty, oldValue, newValue);
    }
    public class ChangedEventArgs<TKey, TType, TValue> : EventArgs where TType : class
    {
        public ChangedEventArgs(TType objectThatChanged, NDReadOnlyPropertyKey<TKey, TType, TValue> changedProperty, TValue oldValue, TValue newValue)
        {
            ChangedObject = objectThatChanged;
            ChangedProperty = changedProperty;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// The old value.
        /// </summary>
        public TValue OldValue { get; }
        /// <summary>
        /// The new value
        /// </summary>
        public TValue NewValue { get; }
        /// <summary>
        /// The Object on which the value is changed.
        /// </summary>
        public TType ChangedObject { get; }
        /// <summary>
        /// The Property that did Change
        /// </summary>
        public NDReadOnlyPropertyKey<TKey, TType, TValue> ChangedProperty { get; }
    }


}
