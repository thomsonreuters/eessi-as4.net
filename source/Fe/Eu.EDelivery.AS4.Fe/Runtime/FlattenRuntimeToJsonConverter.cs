using System;
using System.Collections.Generic;
using System.Linq;
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
            if (rootJson[itemType.Name.ToCamelCase()] != null) return;
            var mainObj = new JObject();
            rootJson.Add(new JProperty(itemType.Name.ToCamelCase(), mainObj));
            foreach (var property in itemType.Properties)
            {
                AddChildren(property, rootJson);
                mainObj.Add(new JProperty(property.TechnicalName.ToCamelCase(), new JObject(new JProperty("description", property.Description))));
            }
        }

        private void AddChildren(Property property, JObject root)
        {
            if (root[property.Type.ToCamelCase()] != null) return;

            var subjObject = new JObject();
            root.Add(new JProperty(property.Type.ToCamelCase(), subjObject));
            subjObject.Add(new JProperty("description", property.Description ?? property.FriendlyName));

            if (property.Properties == null) return;
            foreach (var subProperty in property.Properties)
            {
                subjObject.Add(new JProperty(subProperty.TechnicalName.ToCamelCase(), new JObject(new JProperty("description", subProperty.Description))));
                if (subProperty.Properties != null && subProperty.Properties.Any())
                    AddChildren(subProperty, root);
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
    }
}