using System;

namespace NDProperty
{
    public class NDProperty<TValue, TType> : NDReadOnlyProperty<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly Func<TType, OnChanged<TValue>> changedMethod;

        public NDReadOnlyProperty<TValue, TType> ReadOnlyProperty { get; }

        internal NDProperty(Func<TType, OnChanged<TValue>> changedMethod, bool inherited, NullTreatment nullTreatment, TValue defaultValue, NDPropertySettings settigns) : base(inherited, nullTreatment, defaultValue, settigns)
        {
            ReadOnlyProperty = new NDReadOnlyProperty<TValue, TType>(inherited, nullTreatment, defaultValue, settigns);
            this.changedMethod = changedMethod;
        }
    }


}
