using System;
using NDProperty.Providers;
using NDProperty.Utils;

namespace NDProperty.Propertys
{
    public static class OnChangingArg
    {
        public static OnChangingArg<TValue> Create<TValue>(TValue oldValue, TValue newValue, ValueManager valueManger, bool willBeChanged) => new OnChangingArg<TValue>(oldValue, newValue, valueManger, willBeChanged);
        public static OnChangingArg<TValue, TType> Create<TValue, TType>(TType changedObject, TValue oldValue, TValue newValue, ValueManager valueManger, bool willBeChanged) where TType : class => new OnChangingArg<TValue, TType>(changedObject, oldValue, newValue, valueManger, willBeChanged);
    }
    public class OnChangingArg<TValue>
    {
        public OnChangingArg(TValue oldValue, TValue newValue, ValueManager valueManger, bool willBeChanged)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MutatedValue = newValue;
            WillChange = willBeChanged;
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
        public event ValueChanged ExecuteAfterChange;

        internal void FireExecuteAfterChange()
        {
            ExecuteAfterChange?.Invoke(OldValue, MutatedValue);
        }

        public delegate void ValueChanged(TValue oldValue, TValue newValue);
    }

    public class OnChangingArg<TValue, TType> : OnChangingArg<TValue> where TType : class
    {
        public OnChangingArg(TType changedObject, TValue oldValue, TValue newValue, ValueManager valueManger, bool willBeChanged) : base(oldValue, newValue, valueManger, willBeChanged)
        {
            this.ChangedObject = changedObject;
        }

        /// <summary>
        /// The Object on which the change happend.
        /// </summary>
        public TType ChangedObject { get; }

    }

}
