using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public class FlattenRuntimeToJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = value as IEnumerable<ItemType>;
            var mainObj = new JObject();
            if (list != null)
            {
                foreach (var item in list)
                {
                    WriteItem(item, mainObj);
                }
            }
            else WriteItem((ItemType)value, mainObj);

            mainObj.WriteTo(writer);
        }

        private void WriteItem(ItemType itemType, JObject rootJson)
        {
            // Add all properties to the current JObject
            foreach (var prop in itemType.Properties)
            {
                AddChild(prop, rootJson);
            }
        }

        private void AddChild(Property property, JObject root)
        {
            AddProperty(property, root);
            //root.Add(new JProperty(property.Path, PropertyToJobject(property.Description, property)));
            if (property.Properties == null) return;
            foreach (var childProp in property.Properties)
            {
                AddChild(childProp, root);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        private void AddProperty(Property property, JObject root)
        {
            if (string.IsNullOrEmpty(property.Description) && property.DefaultValue == null) return;
            root.Add(new JProperty(property.Path, new JObject(
                new JProperty("description", property.Description),
                new JProperty("defaultvalue", property.DefaultValue)
            )));
        }

        private JObject PropertyToJobject(string description, Property property)
        {
            return new JObject(
                new JProperty("description", description),
                new JProperty("defaultvalue", property.DefaultValue)
            );
        }
    }
}