using System;
using NDProperty.Providers;
using NDProperty.Utils;

namespace NDProperty.Propertys
{
    public static class OnChangingArg
    {
        public static OnChangingArg<TKey, TValue> Create<TKey, TValue>(TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged) => new OnChangingArg<TKey, TValue>(oldValue, hasOldValue, newValue, hasNewValue, changingProvider, currentProvider, currentValue, willBeChanged);
        public static OnChangingArg<TKey, TValue> Create<TKey, TValue>(TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged, bool canReject) => new OnChangingArg<TKey, TValue>(oldValue, hasOldValue, newValue, hasNewValue, changingProvider, currentProvider, currentValue, willBeChanged, canReject);
        public static OnChangingArg<TKey, TValue, TType> Create<TKey, TValue, TType>(TType changedObject, TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged) where TType : class => new OnChangingArg<TKey, TValue, TType>(changedObject, oldValue, hasOldValue, newValue, hasNewValue, changingProvider, currentProvider, currentValue, willBeChanged);
        public static OnChangingArg<TKey, TValue, TType> Create<TKey, TValue, TType>(TType changedObject, TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged, bool canReject) where TType : class => new OnChangingArg<TKey, TValue, TType>(changedObject, oldValue, hasOldValue, newValue, hasNewValue, changingProvider, currentProvider, currentValue, willBeChanged, canReject);
    }
    public class OnChangingArg<TKey, TValue>
    {
        public OnChangingArg(TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged)
            : this(oldValue, hasOldValue, newValue, hasNewValue, changingProvider, currentProvider, currentValue, willBeChanged, true)
        {

        }
        public OnChangingArg(TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged, bool rejectAllowed)
        {
            CanChange = true; // ofcourse we must allow changes in the constructor ;)
            OldValue = oldValue;
            HasOldValue = hasOldValue;
            HasNewValue = hasNewValue;
            NewValue = newValue;
            MutatedValue = newValue;
            ObjectValueChanging = willBeChanged;
            ChangingProvider = changingProvider;
            CurrentProvider = currentProvider;
            CurrentValue = CurrentValue;
            CanChange = rejectAllowed;
        }

        /// <summary>
        /// The value before the change on the Provider.
        /// </summary>
        internal TValue OldValue { get; }
        /// <summary>
        /// The value after the change on the Provider.
        /// </summary>
        public TValue NewValue { get; }

        /// <summary>
        /// If <c>false</c> the Value was not set yet on this provider.
        /// </summary>
        public bool HasOldValue { get; }
        /// <summary>
        /// If <c>false</c> the Value on this Provider will be deleted.
        /// </summary>
        public bool HasNewValue { get; }

        /// <summary>
        /// The Value this Property currently has.  Will Change to <see cref="NewValue"/> if <see cref="ObjectValueChanging"/> is <c>true</c>.
        /// </summary>
        public TValue CurrentValue { get; }


        /// <summary>
        /// This value can be changed by the called function to transform the value befor it is set on the provider.
        /// </summary>
        /// <remarks>
        /// Deletion of a value can't be mutated.
        /// </remarks>
        /// <exception cref="InvalidOperationException">When CanReject <c>false</c></exception>
        public TValue MutatedValue
        {
            get => mutatedValue;
            set
            {
                if (!CanChange)
                    throw new InvalidOperationException($"{nameof(CanChange)} is {CanChange}. Setting {nameof(MutatedValue)} is not allowed");
                mutatedValue = value;
            }
        }
        private TValue mutatedValue;

        /// <summary>
        /// If set to true the change will not be applied.
        /// </summary>
        /// <remarks>
        /// Deletion of a value can't be rejected.
        /// </remarks>
        /// <exception cref="InvalidOperationException">When CanReject <c>false</c></exception>
        public bool Reject
        {
            get => reject;
            set
            {
                if (!CanChange)
                    throw new InvalidOperationException($"{nameof(CanChange)} is {CanChange}. Setting {nameof(Reject)} is not allowed");
                reject = value;
            }
        }
        private bool reject;


        /// <summary>
        /// Defines if rejecting is and modifing allowed.
        /// </summary>
        public bool CanChange { get; private set; }

        /// <summary>
        /// Determins if the Property will actually change, or if only the value should be veryfied
        /// </summary>
        public bool ObjectValueChanging { get; }

        /// <summary>
        /// The Provider which value is about to change.
        /// </summary>
        public ValueProvider<TKey> ChangingProvider { get; }

        /// <summary>
        /// The Provider that Provides the value before the change.
        /// </summary>
        /// <remarks>
        /// The Provider will cahnge if <see cref="ObjectValueChanging"/> is <c>true</c>.
        /// </remarks>
        public ValueProvider<TKey> CurrentProvider { get; }

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
        public event EventHandler<ValueChangedEventArgs> ExecuteAfterChange;

        internal void FireExecuteAfterChange(object sender)
        {
            ExecuteAfterChange?.Invoke(sender, new ValueChangedEventArgs(NewValue, HasNewValue, OldValue, HasOldValue, ChangingProvider, CurrentProvider, ObjectValueChanging));
        }


        public class ValueChangedEventArgs : EventArgs
        {
            public ValueChangedEventArgs(TValue newValue, bool hasNewValue, TValue oldValue, bool hasOldValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, bool objectValueChanging)
            {
                NewValue = newValue;
                OldValue = oldValue;
                HasNewValue = hasNewValue;
                HasOldValue = hasOldValue;

                ChangingProvider = changingProvider ?? throw new ArgumentNullException(nameof(changingProvider));
                CurrentProvider = currentProvider ?? throw new ArgumentNullException(nameof(currentProvider));
                ObjectValueChanging = objectValueChanging;
            }

            public TValue NewValue { get; }
            public TValue OldValue { get; }
            public ValueProvider<TKey> ChangingProvider { get; }
            public ValueProvider<TKey> CurrentProvider { get; }
            public bool ObjectValueChanging { get; }
            public bool HasNewValue { get; }
            public bool HasOldValue { get; }
        }
    }

    public class OnChangingArg<TKey, TValue, TType> : OnChangingArg<TKey, TValue> where TType : class
    {
        public OnChangingArg(TType changedObject, TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged, bool rejectAllowed) : base(oldValue, hasOldValue, newValue, hasNewValue, changingProvider, currentProvider, currentValue, willBeChanged, rejectAllowed)
        {
            this.ChangedObject = changedObject;
        }
        public OnChangingArg(TType changedObject, TValue oldValue, bool hasOldValue, TValue newValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> currentProvider, TValue currentValue, bool willBeChanged) : base(oldValue, hasOldValue, newValue, hasNewValue, changingProvider, currentProvider, currentValue, willBeChanged)
        {
            this.ChangedObject = changedObject;
        }

        /// <summary>
        /// The Object on which the change happend.
        /// </summary>
        public TType ChangedObject { get; }

    }

}
