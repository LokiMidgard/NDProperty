using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NDProperty.Utils
{
    public static class ConditionalWeakTableHelper
    {

        /// <summary>
        /// Adds or Update a value to a Key.
        /// </summary>
        /// <typeparam name="TKey">The Key</typeparam>
        /// <typeparam name="TValue">The Value</typeparam>
        /// <param name="table">The <see cref="ConditionalWeakTable{TKey, TValue}"/> on which the operation is performed</param>
        /// <param name="key">The key to add. key represents the object to which the property is attached.</param>
        /// <param name="newValue">The key's property value.</param>
        /// <returns>The old value or <c>default(TKey></c></returns>
        public static TValue AddOrUpdate<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> table, TKey key, TValue newValue)
            where TKey : class
            where TValue : class
        {
            if (table.TryGetValue(key, out var oldValue))
            {
                table.Remove(key);
            }
            else
                oldValue = default;

            table.Add(key, newValue);
            return oldValue;
        }

        /// <summary>
        /// Gets or creates a value to a key. If the value is not present on the key, the generator will be used to create the value
        /// </summary>
        /// <typeparam name="TKey">The reference type to which the field is attached.</typeparam>
        /// <typeparam name="TValue">The field's type. This must be a reference type.</typeparam>
        /// <param name="table"></param>
        /// <param name="key">The key to add. key represents the object to which the property is attached.</param>
        /// <param name="valueGenerator">A generator generating the key's property value.</param>
        /// <returns></returns>
        public static TValue GetOrCreateValue<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> table, TKey key, Func<TValue> valueGenerator)
            where TKey : class
            where TValue : class
        {
            if (table.TryGetValue(key, out var oldValue))
                return oldValue;

            var value = valueGenerator();
            table.Add(key, value);
            return value;
        }

    }
}
