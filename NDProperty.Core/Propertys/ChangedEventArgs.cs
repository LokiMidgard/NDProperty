using System;

namespace NDProperty.Propertys
{
    public static class ChangedEventArgs
    {
        public static ChangedEventArgs<TValue, TType> Create<TValue, TType>(TType objectThatChanged, TValue oldValue, TValue newValue) where TType : class => new ChangedEventArgs<TValue, TType>(objectThatChanged, oldValue, newValue);
    }
    public class ChangedEventArgs<TValue, TType> : EventArgs where TType : class
    {
        public ChangedEventArgs(TType objectThatChanged, TValue oldValue, TValue newValue)
        {
            ChangedObject = objectThatChanged;
            this.OldValue = oldValue;
            this.NewValue = newValue;
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
    }


}
