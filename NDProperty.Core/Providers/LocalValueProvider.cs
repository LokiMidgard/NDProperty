using System;
using NDProperty.Propertys;


namespace NDProperty.Providers
{
    public sealed class LocalValueProvider<TKey> : ValueProvider<TKey>
    {
        public static LocalValueProvider<TKey> Instance { get; } = new LocalValueProvider<TKey>();

        private LocalValueProvider() : base(false, true, true)
        {

        }


        public bool SetValue<TType, TValue>(NDBasePropertyKey<TKey, TType, TValue> property, TType changingObject, TValue value)
            where TType : class
        {
            var hasValue = value != null || property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull);

            return Update(changingObject, changingObject, property, value, hasValue, (mutatedValue) =>
            {
                if (mutatedValue == null && !property.Settigns.HasFlag(NDPropertySettings.SetLocalExplicityNull))
                    PropertyRegistar<TKey>.Lookup<TType, TValue>.Property.Remove((changingObject, property));
                else
                    PropertyRegistar<TKey>.Lookup<TType, TValue>.Property[(changingObject, property)] = mutatedValue;

                return true;
            });

        }

        public override (TValue value, bool hasValue) GetValue<TType, TValue>(TType targetObject, NDReadOnlyPropertyKey<TKey, TType, TValue> property)
        {
            var hasValue = PropertyRegistar<TKey>.Lookup<TType, TValue>.Property.ContainsKey((targetObject, property));

            if (hasValue)
                return (PropertyRegistar<TKey>.Lookup<TType, TValue>.Property[(targetObject, property)], hasValue);
            return (default(TValue), hasValue);
        }



        public bool RemoveValue<TType, TValue>(NDBasePropertyKey<TKey, TType, TValue> property, TType changingObject)
              where TType : class
        {
            return Update(changingObject, changingObject, property, default, false, (modifiedValue) =>
            {
                PropertyRegistar<TKey>.Lookup<TType, TValue>.Property.Remove((changingObject, property));
                return true;
            });
        }
    }
}
