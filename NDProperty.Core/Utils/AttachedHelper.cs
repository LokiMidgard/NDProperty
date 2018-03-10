using System;
using System.Collections.Generic;
using System.Text;
using NDProperty.Propertys;

namespace NDProperty.Utils
{

    public static class AttachedHelper
    {
        public static AttachedHelper<TKey, TType, TValue> Create<TKey, TType, TValue>(NDAttachedPropertyKey<TKey, TType, TValue> property) where TType : class => new AttachedHelper<TKey, TType, TValue>(property);
        public static AttachedHelperReadOnly<TKey, TType, TValue> Create<TKey, TType, TValue>(NDReadOnlyPropertyKey<TKey, TType, TValue> property) where TType : class => new AttachedHelperReadOnly<TKey, TType, TValue>(property);
    }

    public class AttachedHelper<TKey, TType, TValue> where TType : class
    {
        private readonly NDAttachedPropertyKey<TKey, TType, TValue> property;

        public AttachedHelper(NDAttachedPropertyKey<TKey, TType, TValue> property)
        {
            this.property = property;
        }

        public Delegater this[TType index]
        {
            get => new Delegater(property, index);
        }

        public class Delegater
        {
            private NDAttachedPropertyKey<TKey, TType, TValue> property;
            private TType index;

            public Delegater(NDAttachedPropertyKey<TKey, TType, TValue> property, TType index)
            {
                this.property = property;
                this.index = index;
            }

            public TValue Value
            {
                get => PropertyRegistar<TKey>.GetValue(property, index);
                set => PropertyRegistar<TKey>.SetValue(property, index, value);
            }

            public event EventHandler<ChangedEventArgs<TKey, TType, TValue>> Changed
            {
                add => PropertyRegistar<TKey>.AddEventHandler(property, value);
                remove => PropertyRegistar<TKey>.RemoveEventHandler(property, index, value);
            }
        }
    }
    public class AttachedHelperReadOnly<TKey, TType, TValue> where TType : class
    {
        private readonly NDReadOnlyPropertyKey<TKey, TType, TValue> property;

        public AttachedHelperReadOnly(NDReadOnlyPropertyKey<TKey, TType, TValue> property)
        {
            this.property = property;
        }

        public Delegater this[TType index]
        {
            get => new Delegater(property, index);
        }

        public class Delegater
        {
            private NDReadOnlyPropertyKey<TKey, TType, TValue> property;
            private TType index;

            public Delegater(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType index)
            {
                this.property = property;
                this.index = index;
            }

            public TValue Value
            {
                get => PropertyRegistar<TKey>.GetValue(property, index);
            }

            public event EventHandler<ChangedEventArgs<TKey, TType, TValue>> Changed
            {
                add => PropertyRegistar<TKey>.AddEventHandler(property, value);
                remove => PropertyRegistar<TKey>.RemoveEventHandler(property, index, value);
            }
        }
    }
}
