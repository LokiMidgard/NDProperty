using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NDProperty.Propertys;


namespace NDProperty.Providers
{

    public abstract class ValueProvider<TKey>
    {
        protected bool Update<TValue, TType, TPropertyType>(TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var (oldValue, otherProvider) = PropertyRegistar<TKey>.GetValueAndProvider(property, targetObject);

            return Update(targetObject, targetObject, property, value, updateCode, oldValue, otherProvider);
        }

        internal bool Update<TValue, TType, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, Func<bool> updateCode, TValue oldValue, ValueProvider<TKey> otherProvider)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey,TValue, TType>, INDProperty<TKey, TValue, TType>
        {
            var otherProviderIndex = PropertyRegistar<TKey>.ManagerOrder[otherProvider];
            var thisIndex = PropertyRegistar<TKey>.ManagerOrder[this];

            // We need to call the actial update after we recived the current old value. Otherwise we could already read the 
            OnChangingArg<TKey, TValue> onChangingArg;
            if (property as object is NDAttachedPropertyKey<TKey, TValue, TType> attach)
            {
                var attachArg = OnChangingArg.Create(targetObject, oldValue, value, this, otherProviderIndex >= thisIndex); ;
                onChangingArg = attachArg;
                attach.changedMethod(attachArg);
            }
            else if (property as object is NDPropertyKey<TKey, TValue, TType> p)
            {
                onChangingArg = OnChangingArg.Create(oldValue, value, this, otherProviderIndex >= thisIndex);
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
