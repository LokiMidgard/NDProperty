using System;
using NDProperty.Providers;
using NDProperty.Utils;

namespace NDProperty.Propertys
{
    public static class OnChangingArg
    {
        public static OnChangingArg<TKey, TValue> Create<TKey, TValue>(TValue oldValue, TValue newValue, ValueProvider<TKey> newProvider, ValueProvider<TKey> oldProvider, bool willBeChanged) => new OnChangingArg<TKey, TValue>(oldValue, newValue, newProvider, oldProvider, willBeChanged);
        public static OnChangingArg<TKey, TValue, TType> Create<TKey, TValue, TType>(TType changedObject, TValue oldValue, TValue newValue, ValueProvider<TKey> newProvider, ValueProvider<TKey> oldProvider, bool willBeChanged) where TType : class => new OnChangingArg<TKey, TValue, TType>(changedObject, oldValue, newValue, newProvider, oldProvider, willBeChanged);
    }
    public class OnChangingArg<TKey, TValue>
    {
        public OnChangingArg(TValue oldValue, TValue newValue, ValueProvider<TKey> newProvider, ValueProvider<TKey> oldProvider, bool willBeChanged)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MutatedValue = newValue;
            WillChange = willBeChanged;
            NewProvider = newProvider;
            OldProvider = oldProvider;
        }

        /// <summary>
        /// The value before the change.
        /// </summary>
        internal TValue OldValue { get; }
        /// <summary>
        /// The value after the change.
        /// </summary>
        public TValue NewValue { get; }

        /// <summary>
        /// This value can be changed by the called function to transform the value befor it is set.
        /// </summary>
        public TValue MutatedValue { get; set; }

        /// <summary>
        /// If set to true the change will not be applied.
        /// </summary>
        public bool Reject { get; set; }

        /// <summary>
        /// Determins if the Property will actually change, or if only the value should be veryfied
        /// </summary>
        public bool WillChange { get; }
        public ValueProvider<TKey> NewProvider { get; }

        /// <summary>
        /// Set this Property to imply an data error.
        /// </summary>
        public StringResource Error { get; set; }
        public ValueProvider<TKey> OldProvider { get; }

        /// <summary>
        /// Register at this event if you need to perform an action after the value was change on the Proeprty.
        /// </summary>
        /// <remarks>
        /// The eventhandler will be called before the value is changed. Accessing the Property at this time wil result in the old value.<para/>
        /// If you need to call methods that will access this property and the methods need the new value, call those metheds in an Action that is
        /// registert at this event.
        /// </remarks>
        public event EventHandler<ValueChangedEventArgs> ExecuteAfterChange;

        internal void FireExecuteAfterChange(object sender)
        {
            ExecuteAfterChange?.Invoke(sender, new ValueChangedEventArgs(NewValue, OldValue, NewProvider, OldProvider));
        }


        public class ValueChangedEventArgs : EventArgs
        {
            public ValueChangedEventArgs(TValue newValue, TValue oldValue, ValueProvider<TKey> newProvider, ValueProvider<TKey> oldProvider)
            {
                NewValue = newValue;
                OldValue = oldValue;
                NewProvider = newProvider ?? throw new ArgumentNullException(nameof(newProvider));
                OldProvider = oldProvider ?? throw new ArgumentNullException(nameof(oldProvider));
            }

            public TValue NewValue { get; }
            public TValue OldValue { get; }
            public ValueProvider<TKey> NewProvider { get; }
            public ValueProvider<TKey> OldProvider { get; }
        }
    }

    public class OnChangingArg<TKey, TValue, TType> : OnChangingArg<TKey, TValue> where TType : class
    {
        public OnChangingArg(TType changedObject, TValue oldValue, TValue newValue, ValueProvider<TKey> newProvider, ValueProvider<TKey> oldProvider, bool willBeChanged) : base(oldValue, newValue, newProvider, oldProvider, willBeChanged)
        {
            this.ChangedObject = changedObject;
        }

        /// <summary>
        /// The Object on which the change happend.
        /// </summary>
        public TType ChangedObject { get; }

    }

}
