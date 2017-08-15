using System;
using System.Collections.Generic;
using System.Text;
using NDProperty.Providers;

namespace NDProperty
{
    public interface IInitilizer<TKey>
    {
        IEnumerable<ValueProvider<TKey>> ValueProvider { get; }
    }

class MyConfiguration : IInitilizer<MyConfiguration>
{
    public IEnumerable<ValueProvider<MyConfiguration>> ValueProvider => new ValueProvider<MyConfiguration>[] {
        NDProperty.Providers.LocalValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.InheritenceValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.DefaultValueProvider<MyConfiguration>.Instance,
    };
}
}
