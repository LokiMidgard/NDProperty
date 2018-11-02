using System;
using System.Collections.Generic;
using System.Text;
using NDProperty.Providers;

namespace NDProperty
{
    public interface IInitializer<TKey>
    {
        IEnumerable<ValueProvider<TKey>> ValueProviders { get; }
    }

    internal class MyConfiguration : IInitializer<MyConfiguration>
    {
        public IEnumerable<ValueProvider<MyConfiguration>> ValueProviders { get; } = new ValueProvider<MyConfiguration>[] {
        NDProperty.Providers.LocalValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.InheritanceValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.DefaultValueProvider<MyConfiguration>.Instance,
    };
    }
}
