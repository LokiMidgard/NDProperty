using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NDProperty.Propertys;


namespace NDProperty.Providers
{
    /// <summary>
    /// Base class to provide aloternative value lookup for NDP Propertys
    /// </summary>
    /// <typeparam name="TKey">The Configuration</typeparam>
    public abstract class ValueProvider<TKey>
    {
        protected bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var (oldValue, otherProvider) = PropertyRegistar<TKey>.GetValueAndProvider(property, targetObject);

            return Update(sender, targetObject, property, value, updateCode, oldValue, otherProvider);
        }

        protected bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue newValue, TValue oldValue, ValueProvider<TKey> oldProvider)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            return Update(sender, targetObject, property, newValue, () => true, oldValue, oldProvider);
        }
        internal bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode, TValue oldValue, ValueProvider<TKey> oldProvider)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey,TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var otherProviderIndex = PropertyRegistar<TKey>.ProviderOrder[oldProvider];
            var thisIndex = PropertyRegistar<TKey>.ProviderOrder[this];

            // We need to call the actial update after we recived the current old value. Otherwise we could already read the 
            OnChangingArg<TKey, TValue> onChangingArg;
            if (property as object is NDAttachedPropertyKey<TKey, TValue, TType> attach)
            {
                var attachArg = OnChangingArg.Create(targetObject, oldValue, value, oldProvider, this, otherProviderIndex >= thisIndex); ;
                onChangingArg = attachArg;
                attach.changedMethod(attachArg);
            }
            else if (property as object is NDPropertyKey<TKey, TValue, TType> p)
            {
                onChangingArg = OnChangingArg.Create(oldValue, value, oldProvider,this, otherProviderIndex >= thisIndex);
                p.changedMethod(targetObject)(onChangingArg);
            }
            else
                throw new NotSupportedException();
            return PropertyRegistar<TKey>.ChangeValue(sender, property, targetObject, onChangingArg, updateCode);
        }

        public abstract (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TKey, TValue, TType> property) where TType : class;
        //public abstract bool HasValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TValue, TType> property) where TType : class;
    }


}
