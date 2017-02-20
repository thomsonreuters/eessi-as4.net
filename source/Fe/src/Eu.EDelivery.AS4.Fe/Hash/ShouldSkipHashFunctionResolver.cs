using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Eu.EDelivery.AS4.Fe.Hash
{
    public class ShouldSkipHashFunctionResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.PropertyName == "Hash") property.Ignored = true;
            return property;
        }
    }
}