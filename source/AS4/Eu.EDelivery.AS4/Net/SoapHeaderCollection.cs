using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Net
{
    internal class SoapHeaderCollection : IList<XmlElement>
    {
        private readonly List<XmlElement> _rawHeaders =
            new List<XmlElement>();

        private Messaging _messagingHeader;

        public Messaging this[AS4Header header]
        {
            get { return this._messagingHeader; }
            set { this._messagingHeader = value; }
        }

        public XmlElement this[int index]
        {
            get { return this._rawHeaders[index]; }
            set { this._rawHeaders[index] = value; }
        }

        /// <summary>
        /// Gets the number of elements contained in the list
        /// </summary>
        public int Count => this._rawHeaders.Count;

        /// <summary>
        /// Gets a value indicating whether the list is read-only
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an item to the list
        /// </summary>
        /// <param name="item"></param>
        public void Add(XmlElement item) => this._rawHeaders.Add(item);

        /// <summary>
        /// Removes all items from the list
        /// </summary>
        public void Clear()
        {
            this._rawHeaders.Clear();
            this._messagingHeader = null;
        }

        /// <summary>
        /// Determines whether the list contains a specific value
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(XmlElement item)
        {
            return this._rawHeaders.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the list to an array,
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(XmlElement[] array, int arrayIndex)
        {
            this._rawHeaders.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<XmlElement> GetEnumerator()
        {
            return this._rawHeaders.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(XmlElement item)
        {
            return this._rawHeaders.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the list at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, XmlElement item)
        {
            this._rawHeaders.Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(XmlElement item)
        {
            return this._rawHeaders.Remove(item);
        }

        /// <summary>
        /// Removes the item at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            this._rawHeaders.RemoveAt(index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._rawHeaders.GetEnumerator();
        }
    }
}