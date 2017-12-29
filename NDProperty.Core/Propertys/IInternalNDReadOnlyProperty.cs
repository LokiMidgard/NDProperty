using NDProperty.Providers;

namespace NDProperty.Propertys
{
    internal interface IInternalNDProperty<TKey> : IInternalNDReadOnlyProperty<TKey>
    {
        void CallSetOmInHeritanceProvider(object affectedObject, object source, object value, bool v, object oldValue, bool hasOldValue, ValueProvider<TKey> currentProvider, object currentValue);

    }
    internal interface IInternalNDReadOnlyProperty<TKey>
    {

        (object, ValueProvider<TKey>) GetValueAndProvider(object obj);
        (object value, bool hasValue) GetProviderValue(object obj, ValueProvider<TKey> provider);
        void CallChangeHandler(object obj, object sender, object oldValue, object newValue);
    }


}
