using System;

namespace NDProperty.Providers
{
    internal interface IValueWillChangeEventArgs<out TValue>
    {
        bool HasValue { get; }
        TValue NewValue { get; }

        event Action AfterChange;
    }
}