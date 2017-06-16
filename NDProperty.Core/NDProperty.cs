using System;

namespace NDProperty
{
    public class NDProperty<TValue, TType> : NDReadOnlyProperty<TValue, TType>, INDProperty<TValue, TType> where TType : class
    {
        internal readonly Func<TType, OnChanged<TValue>> changedMethod;

        public NDReadOnlyProperty<TValue, TType> ReadOnlyProperty { get; }

        internal NDProperty(Func<TType, OnChanged<TValue>> changedMethod, bool inherited, NullTreatment nullTreatment) : base(inherited, nullTreatment)
        {
            ReadOnlyProperty = new NDReadOnlyProperty<TValue, TType>(inherited, nullTreatment);
            this.changedMethod = changedMethod;
        }
    }


}
