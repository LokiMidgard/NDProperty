using System;
using NDProperty.Providers;

namespace NDProperty.Propertys
{
    /// <summary>
    /// This key alows read and write access to an NDProperty.
    /// </summary>
    /// <typeparam name="TValue">The type of the Property</typeparam>
    /// <typeparam name="TType">The type of the Object that defines the Property.</typeparam>
    public class NDPropertyKey<TKey, TType, TValue> : NDBasePropertyKey<TKey, TType, TValue>,  IInternalNDProperty<TKey>
        where TType : class
    {
        internal readonly Func<TType, OnChanging<TKey, TValue>> changedMethod;
        private readonly Action<TType> notifyPropertyChanged;

        internal NDPropertyKey(Func<TType, OnChanging<TKey, TValue>> changedMethod, TValue defaultValue, NDPropertySettings settigns, Action<TType> notifyPropertyChanged) : base(defaultValue, settigns)
        {
            this.changedMethod = changedMethod;
            this.notifyPropertyChanged = notifyPropertyChanged;
        }

        internal void FirePropertyChanged(TType obj) => this.notifyPropertyChanged?.Invoke(obj);

        void IInternalNDProperty<TKey>.CallSetOmInHeritanceProvider(object obj, object source, object newValue, bool hasNewValue, object oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, object currentValue)
        {
            if (obj is TType t && source is TType sourceObject)
                InheritenceValueProvider<TKey>.Instance.SetValue(t, this, sourceObject, (TValue)newValue, hasNewValue, (TValue)oldValue, hasOldValue, currentProvider, (TValue)currentValue);
            else
                throw new ArgumentException($"Parameter was not of Type {typeof(TType).FullName}");
        }
    }


}
