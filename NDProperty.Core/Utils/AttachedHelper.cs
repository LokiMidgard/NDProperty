using System;
using System.Collections.Generic;
using System.Text;

namespace NDProperty.Utils
{

    public static class AttachedHelper
    {
        public static AttachedHelper<TValue, TType> Create<TValue, TType>(NDAttachedPropertyKey<TValue, TType> property) where TType : class => new AttachedHelper<TValue, TType>(property);
    }
    public class AttachedHelper<TValue, TType> where TType : class
    {
        private readonly NDAttachedPropertyKey<TValue, TType> property;

        public AttachedHelper(NDAttachedPropertyKey<TValue, TType> property)
        {
            this.property = property;
        }

        public Delegater this[TType index]
        {
            get => new Delegater(property, index);
        }

        public class Delegater
        {
            private NDAttachedPropertyKey<TValue, TType> property;
            private TType index;

            public Delegater(NDAttachedPropertyKey<TValue, TType> property, TType index)
            {
                this.property = property;
                this.index = index;
            }

            public TValue Value
            {
                get => PropertyRegistar.GetValue(property, index);
                set => PropertyRegistar.SetValue(property, index, value);
            }

            public event EventHandler<ChangedEventArgs<TValue, TType>> Changed
            {
                add => PropertyRegistar.AddEventHandler(property, value);
                remove => PropertyRegistar.RemoveEventHandler(property, index, value);
            }
        }
    }
}
