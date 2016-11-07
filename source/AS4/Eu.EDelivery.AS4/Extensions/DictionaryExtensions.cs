using System.Collections.Generic;
using System.Text;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Extensions
{
    /// <summary>
    /// Extensions class to help with <see cref="IDictionary{TKey, TValue}"/> implementations,
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
        /// <exception cref="AS4Exception"></exception>
        /// <returns></returns>
        public static TValue ReadMandatoryProperty<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key)
        {
            if (!dictionary.ContainsKey(key)) throw new AS4Exception($"Dictionary doesn't contain key: {key}");
            if (dictionary[key] == null) throw new AS4Exception($"Dictionary contains empty value for key: {key}");
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
        public static string ReadOptionalProperty<TKey>(
            this IDictionary<TKey, string> dictionary,
            TKey key)
        {
            return dictionary.ReadOptionalProperty(key, string.Empty);
        }

        /// <summary>
        /// Returns the given <see cref="IDictionary{TKey, TValue}"/> as a string
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string Flatten<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
                return string.Empty;

            var buffer = new StringBuilder();
            foreach (TKey key in dictionary.Keys)
            {
                buffer.AppendFormat("{0}:{1};", key, dictionary[key]);
            }

            return buffer.ToString(0, buffer.Length - 1);
        }

        /// <summary>
        /// Replaces the value if the key exists, otherwise adds the key-value.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Merge<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else dictionary.Add(key, value);
        }

        /// <summary>
        /// Merges the key-values of additional with the key-values of this dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="additional"></param>
        public static void Merge<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            IDictionary<TKey, TValue> additional)
        {
            foreach (TKey key in additional.Keys)
                dictionary.Merge(key, additional[key]);
        }

        /// <summary>
        /// Removes the item from the dictionary with the given key.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        public static void Demote<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key)
        {
            if (!dictionary.ContainsKey(key))
                return;

            dictionary.Remove(key);
        }
    }
}