using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.PMode
{
    [Serializable]
    public class Method
    {
        public string Type { get; set; }
        public List<Parameter> Parameters { get; set; }

        public Parameter this[string name] => GetParameter(name);

        private Parameter GetParameter(string name)
        {
            return this.Parameters?.FirstOrDefault(p 
                => p?.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) == true);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string parameters =
                Parameters == null
                    ? "<null>"
                    : Parameters.Count == 0
                        ? "[]"
                        : $"[{String.Join("; ", Parameters.Select(p => p == null ? "<null>" : $"Name:{p.Name ?? "<null>"},Value={p.Value ?? "<null>"}"))}]";

            return $"Type: {Type ?? "<null>"}, Parameters: {parameters}";
        }
    }

    public class Parameter
    {
        [XmlAttribute(attributeName:"name")]
        public string Name { get; set; }
        [XmlAttribute(attributeName:"value")]
        public string Value { get; set; }
    }
}