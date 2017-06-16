using System;
using System.Collections.Generic;
using System.Text;

namespace NDProperty
{

    public static partial class PropertyRegistar
    {

        public static class AttachedHelper
        {
            public static AttachedHelper<TValue, TType> Create<TValue, TType>(NDAttachedProperty<TValue, TType> property) where TType : class => new AttachedHelper<TValue, TType>(property);
        }
        public class AttachedHelper<TValue, TType> where TType : class
        {
            private readonly NDAttachedProperty<TValue, TType> property;

            public AttachedHelper(NDAttachedProperty<TValue, TType> property)
            {
                this.property = property;
            }

            public Delegater this[TType index]
            {
                get => new Delegater(property, index);
            }

            public class Delegater
            {
                private NDAttachedProperty<TValue, TType> property;
                private TType index;

                public Delegater(NDAttachedProperty<TValue, TType> property, TType index)
                {
                    this.property = property;
                    this.index = index;
                }

                public TValue Value
                {
                    get => GetValue(property, index);
                    set => SetValue(property, index, value);
                }

                public event EventHandler<ChangedEventArgs<TValue, TType>> Changed
                {
                    add => AddEventHandler(property, value);
                    remove => RemoveEventHandler(property, index, value);
                }
            }
        }
    }
}
