using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Model.PMode
{
    public class MessageHandlingConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = JObject.Load(reader);
            if (result["sendingPMode"] != null)
            {
                return result.ToObject<Forward>();
            }

            else if (result["isEnabled"] != null)
            {
                return result.ToObject<Deliver>();
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}