using System;

namespace NDProperty
{
    public class NDPropertyKey<TValue, TType> : NDReadOnlyPropertyKey<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly Func<TType, OnChanged<TValue>> changedMethod;

        public NDReadOnlyPropertyKey<TValue, TType> ReadOnlyProperty { get; }

        internal NDPropertyKey(Func<TType, OnChanged<TValue>> changedMethod, NullTreatment nullTreatment, TValue defaultValue, NDPropertySettings settigns) : base(nullTreatment, defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyPropertyKey<TValue, TType>(nullTreatment, defaultValue, settigns);
            this.changedMethod = changedMethod;
        }
    }


}
