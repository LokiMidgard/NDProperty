using System;
using System.Collections.Generic;
using System.Text;
using NDProperty.Providers;

namespace NDProperty
{
    public interface IInitilizer<TKey>
    {
        IEnumerable<ValueManager<TKey>> ValueProvider { get; }
    }
}
