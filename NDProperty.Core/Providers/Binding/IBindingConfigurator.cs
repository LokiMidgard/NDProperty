using System;
using NDProperty.Propertys;

namespace NDProperty.Providers.Binding
{
    public interface IBindingConfigurator<TKey, TType, TValue> where TType : class
    {
        IBindingConfiguration<TKey, TSourceValue, TType, TValue> ConvertOneWay<TSourceValue>(Func<TValue, TSourceValue> converter);
        IBindingConfiguration<TKey, TValue, TType, TValue> OneWay();

        IBindingConfiguratorWritable<TKey, TNewType, TNewValue> Over<TNewType, TNewValue>(NDBasePropertyKey<TKey, TNewType, TNewValue> property) where TNewType : class, TValue;
        IBindingConfigurator<TKey, TNewType, TNewValue> Over<TNewType, TNewValue>(NDReadOnlyPropertyKey<TKey, TNewType, TNewValue> property) where TNewType : class, TValue;
    }
}