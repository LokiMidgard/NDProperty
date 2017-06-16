using System;

namespace NDProperty
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

        public TValue OldValue { get; }
        public TValue NewValue { get; }
        public TType ChangedObject { get; }
    }


}
