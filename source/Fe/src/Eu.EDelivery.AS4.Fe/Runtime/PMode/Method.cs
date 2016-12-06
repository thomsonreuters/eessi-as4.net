using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.PMode
{
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
    }

    public class Parameter
    {
        [XmlAttribute(attributeName:"name")]
        public string Name { get; set; }
        [XmlAttribute(attributeName:"value")]
        public string Value { get; set; }
    }
}