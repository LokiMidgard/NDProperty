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

        public TValue OldValue { get; }
        public TValue NewValue { get; }

        /// <summary>
        /// This value can be changed by the called function to transform the value befor it is set.
        /// </summary>
        public TValue MutatedValue { get; set; }

        public bool Reject { get; set; }

        public StringResource Error { get; set; }

        public event Action ExecuteAfterChange;

    }

    public class OnChangedArg<TValue, TType> : OnChangedArg<TValue> where TType : class
    {
        public OnChangedArg(TType changedObject, TValue oldValue, TValue newValue) : base(oldValue, newValue)
        {
            this.ChangedObject = changedObject;
        }

        public TType ChangedObject { get; }

    }

}
