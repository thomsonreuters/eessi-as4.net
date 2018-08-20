using System.Collections.Generic;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Extensions
{
    /// <summary>
    /// Extensions class to help with <see cref="IDictionary{TKey,TValue}" /> implementations,
    /// provide standard CRUD operations
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Read a required property
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue ReadMandatoryProperty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (!dictionary.ContainsKey(key))
            {
                throw new KeyNotFoundException($"Dictionary doesn't contain key: {key}");
            }

            if (dictionary[key] == null)
            {
                throw new KeyNotFoundException($"Dictionary contains empty value for key: {key}");
            }

            return dictionary[key];
        }

        /// <summary>
        /// Read an optional property with provided default value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue ReadOptionalProperty<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            return !dictionary.ContainsKey(key) ? defaultValue : dictionary[key];
        }

        /// <summary>
        /// Read an optional property with a empty string default value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadOptionalProperty<TKey>(this IDictionary<TKey, string> dictionary, TKey key)
        {
            return dictionary.ReadOptionalProperty(key, string.Empty);
        }
    }
}