using System;
using NDProperty.Providers;
using NDProperty.Utils;

namespace NDProperty.Propertys
{
    public static class OnChangingArg
    {
        public static OnChangingArg<TKey, TValue> Create<TKey, TValue>(TValue oldProviderValue, bool hasOldValue, TValue newProviderValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> oldProvider, ValueProvider<TKey> newProvider, TValue oldPropertyValue, TValue newPropertyValue, bool rejectAllowed) => new OnChangingArg<TKey, TValue>(oldProviderValue, hasOldValue, newProviderValue, hasNewValue, changingProvider, oldProvider, newProvider, oldPropertyValue, newPropertyValue, rejectAllowed);
        public static OnChangingArg<TKey, TType, TValue> Create<TKey, TType, TValue>(TType changedObject, TValue oldProviderValue, bool hasOldValue, TValue newProviderValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> oldProvider, ValueProvider<TKey> newProvider, TValue oldPropertyValue, TValue newPropertyValue, bool rejectAllowed) where TType : class => new OnChangingArg<TKey, TType, TValue>(changedObject, oldProviderValue, hasOldValue, newProviderValue, hasNewValue, changingProvider, oldProvider, newProvider, oldPropertyValue, newPropertyValue, rejectAllowed);
    }
    public class OnChangingArg<TKey, TValue>
    {
        private TValue newActualValue;
        private bool didChange;

        public OnChangingArg(TValue oldProviderValue, bool hasOldValue, TValue newProviderValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> oldProvider, ValueProvider<TKey> newProvider, TValue oldPropertyValue, TValue newPropertyValue, bool rejectAllowed)
        {
            Property = new PropertyChanged(this, oldPropertyValue, newPropertyValue, oldProvider, newProvider);
            Provider = new ProviderChanged(this, changingProvider, rejectAllowed, oldProviderValue, newProviderValue, hasOldValue, hasNewValue);
        }

        /// <summary>
        /// Capsuls Information on Changes of the Property of the Object.
        /// </summary>
        public PropertyChanged Property { get; }

        /// <summary>
        /// Capsuls Information on Changes of the Provider that is currently changing.
        /// </summary>
        public ProviderChanged Provider { get; }

        /// <summary>
        /// Register at this event if you need to perform an action after the value was change on the Proeprty.
        /// </summary>
        /// <remarks>
        /// The eventhandler will be called before the value is changed. Accessing the Property at this time wil result in the old value.<para/>
        /// If you need to call methods that will access this property and the methods need the new value, call those metheds in an Action that is
        /// registert at this event.<para/>
        /// This Handler will also be fired if the actual property of the Object will not change.
        /// </remarks>
        public event EventHandler<ValueChangedEventArgs> ExecuteAfterChange;

        internal void FireExecuteAfterChange(object sender)
        {
            ExecuteAfterChange?.Invoke(sender, new ValueChangedEventArgs(Property, Provider));
        }

        public class ProviderChanged
        {
            public ProviderChanged(OnChangingArg<TKey, TValue> parent, ValueProvider<TKey> changingProvider, bool rejectAllowed, TValue oldValue, TValue newValue, bool hasOldValue, bool hasNewValue)
            {
                this.parent = parent;
                CanChange = true;
                HasOldValue = hasOldValue;
                HasNewValue = hasNewValue;
                OldValue = oldValue;
                NewValue = newValue;
                MutatedValue = newValue;
                ChangingProvider = changingProvider;
                CanChange = rejectAllowed;
            }

            /// <summary>
            /// The provider that is changing.
            /// </summary>
            public ValueProvider<TKey> ChangingProvider { get; }

            /// <summary>
            /// The value before the change on the Provider.
            /// </summary>
            public TValue OldValue { get; }
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
            /// This value can be changed by the called function to transform the value befor it is set on the provider.
            /// </summary>
            /// <remarks>
            /// Deletion of a value can't be mutated.
            /// </remarks>
            /// <exception cref="InvalidOperationException">When CanReject <c>false</c></exception>
            public TValue MutatedValue
            {
                get => this.mutatedValue;
                set
                {
                    if (!CanChange)
                        throw new InvalidOperationException($"{nameof(CanChange)} is {CanChange}. Setting {nameof(MutatedValue)} is not allowed");
                    this.mutatedValue = value;
                    if (ChangingProvider == this.parent.Property.NewProvider) // we need to publicate the mutation to Property
                    {
                        this.parent.newActualValue = value;
                        this.parent.didChange = !Equals(this.parent.Property.OldValue, this.parent.Property.NewValue);
                    }
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
                get => this.reject;
                set
                {
                    if (!CanChange)
                        throw new InvalidOperationException($"{nameof(CanChange)} is {CanChange}. Setting {nameof(Reject)} is not allowed");
                    this.reject = value;
                }
            }
            private bool reject;
            private readonly OnChangingArg<TKey, TValue> parent;

            /// <summary>
            /// Defines if rejecting is and modifing allowed.
            /// </summary>
            public bool CanChange { get; private set; }
        }

        public class PropertyChanged
        {
            public PropertyChanged(OnChangingArg<TKey, TValue> parent, TValue oldValue, TValue newValue, ValueProvider<TKey> oldProvider, ValueProvider<TKey> newProvider)
            {
                this.parent = parent;
                OldProvider = oldProvider;
                NewProvider = newProvider;
                OldValue = oldValue;
                parent.newActualValue = newValue;
                parent.didChange = !Equals(OldValue, NewValue);
            }

            /// <summary>
            /// The value of the Property before the change on the Provider.
            /// </summary>
            public TValue OldValue { get; }
            /// <summary>
            /// The value of the Property after the change on the Provider.
            /// </summary>
            public TValue NewValue => this.parent.newActualValue;


            /// <summary>
            /// The Provider that provides the Value for the Property after the change.
            /// </summary>
            public ValueProvider<TKey> NewProvider { get; }

            private readonly OnChangingArg<TKey, TValue> parent;

            /// <summary>
            /// The Provider that provides the Value for the Property befor the change.
            /// </summary>
            public ValueProvider<TKey> OldProvider { get; }


            /// <summary>
            /// Determins if the Property will actually change, or if only the value should be veryfied
            /// </summary>
            public bool IsObjectValueChanging => this.parent.didChange;

        }

        public class ValueChangedEventArgs : EventArgs
        {
            public ValueChangedEventArgs(PropertyChanged property, ProviderChanged provider)
            {
                Property = property;
                Provider = provider;
            }

            public PropertyChanged Property { get; }

            public ProviderChanged Provider { get; }

        }
    }

    public class OnChangingArg<TKey, TType, TValue> : OnChangingArg<TKey, TValue> where TType : class
    {
        public OnChangingArg(TType changedObject, TValue oldProviderValue, bool hasOldValue, TValue newProviderValue, bool hasNewValue, ValueProvider<TKey> changingProvider, ValueProvider<TKey> oldProvider, ValueProvider<TKey> newProvider, TValue oldPropertyValue, TValue newPropertyValue, bool rejectAllowed) : base(oldProviderValue, hasOldValue, newProviderValue, hasNewValue, changingProvider, oldProvider, newProvider, oldPropertyValue, newPropertyValue, rejectAllowed)
        {
            this.ChangedObject = changedObject;
        }

        /// <summary>
        /// The Object on which the change happend.
        /// </summary>
        public TType ChangedObject { get; }

    }

}
