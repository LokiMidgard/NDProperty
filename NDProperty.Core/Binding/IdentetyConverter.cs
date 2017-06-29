using System;

namespace NDProperty.Binding
{
    internal class IdentetyConverter<TValue> : ITwoWayConverter<TValue, TValue>
    {
        public TValue ConvertTo(TValue source)
        {
            return source;
        }
    }
}