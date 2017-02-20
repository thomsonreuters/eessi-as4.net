using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Fe.Hash
{
    public static class ObjectExtensions
    {
        public static string GetMd5Hash(this object obj)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ContractResolver = new ShouldSkipHashFunctionResolver()
                }));
                var sb = new StringBuilder();
                var hash = md5.ComputeHash(inputBytes);
                foreach (var t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}