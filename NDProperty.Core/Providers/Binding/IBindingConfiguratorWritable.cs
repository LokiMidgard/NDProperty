using System;

namespace NDProperty.Providers.Binding
{
    public interface IBindingConfiguratorWritable<TKey, TType, TValue> :IBindingConfigurator<TKey, TType, TValue>
        where TType : class
    {
        // Not yet Implemented...
        IBindingConfiguration<TKey, TSourceValue, TType, TValue> ConvertTwoWay<TSourceValue>(Func<TValue, TSourceValue> converter, Func<TSourceValue, TValue> converterback);
        IBindingConfiguration<TKey, TValue, TType, TValue> TwoWay();
    }
}