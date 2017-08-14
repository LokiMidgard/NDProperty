using System;
using System.Collections.Generic;
using System.Text;
using NDProperty.Propertys;

namespace NDProperty.Utils
{

    public static class AttachedHelper
    {
        public static AttachedHelper<TKey, TValue, TType> Create<TKey, TValue, TType>(NDAttachedPropertyKey<TKey, TValue, TType> property) where TType : class => new AttachedHelper<TKey, TValue, TType>(property);
    }
    public class AttachedHelper<TKey, TValue, TType> where TType : class
    {
        private readonly NDAttachedPropertyKey<TKey, TValue, TType> property;

        public AttachedHelper(NDAttachedPropertyKey<TKey, TValue, TType> property)
        {
            this.property = property;
        }

        public Delegater this[TType index]
        {
            get => new Delegater(property, index);
        }

        public class Delegater
        {
            private NDAttachedPropertyKey<TKey, TValue, TType> property;
            private TType index;

            public Delegater(NDAttachedPropertyKey<TKey, TValue, TType> property, TType index)
            {
                this.property = property;
                this.index = index;
            }

            public TValue Value
            {
                get => PropertyRegistar<TKey>.GetValue(property, index);
                set => PropertyRegistar<TKey>.SetValue(property, index, value);
            }

            public event EventHandler<ChangedEventArgs<TValue, TType>> Changed
            {
                add => PropertyRegistar<TKey>.AddEventHandler(property, value);
                remove => PropertyRegistar<TKey>.RemoveEventHandler(property, index, value);
            }
        }
    }
}
