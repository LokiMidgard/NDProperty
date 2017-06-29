using System;

namespace NDProperty
{
    public static class OnChangedArg
    {
        public static OnChangedArg<TValue> Create<TValue>(TValue oldValue, TValue newValue) => new OnChangedArg<TValue>(oldValue, newValue);
        public static OnChangedArg<TValue, TType> Create<TValue, TType>(TType changedObject, TValue oldValue, TValue newValue) where TType : class => new OnChangedArg<TValue, TType>(changedObject, oldValue, newValue);
    }
    public class OnChangedArg<TValue>
    {
        public OnChangedArg(TValue oldValue, TValue newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MutatedValue = newValue;
        }

        /// <summary>
        /// The value before the change.
        /// </summary>
        public TValue OldValue { get; }
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
        /// Set this Property to imply an data error.
        /// </summary>
        public StringResource Error { get; set; }

        /// <summary>
        /// Register at this event if you need to perform an action after the value was change on the Proeprty.
        /// </summary>
        /// <remarks>
        /// The eventhandler will be called before the value is changed. Accessing the Property at this time wil result in the old value.<para/>
        /// If you need to call methods that will access this property and the methods need the new value, call those metheds in an Action that is
        /// registert at this event.
        /// </remarks>
        public event Action ExecuteAfterChange;

        internal void FireExecuteAfterChange()
        {
            ExecuteAfterChange?.Invoke();
        }

    }

    public class OnChangedArg<TValue, TType> : OnChangedArg<TValue> where TType : class
    {
        public OnChangedArg(TType changedObject, TValue oldValue, TValue newValue) : base(oldValue, newValue)
        {
            this.ChangedObject = changedObject;
        }

        /// <summary>
        /// The Object on which the change happend.
        /// </summary>
        public TType ChangedObject { get; }

    }

}
