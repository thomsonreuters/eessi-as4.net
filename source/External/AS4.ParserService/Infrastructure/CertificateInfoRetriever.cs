using System;
using System.Net.Http;
using System.Text;

namespace AS4.ParserService.Infrastructure
{
    internal static class CertificateInfoRetriever
    {
        internal static CertificatePwdInformation RetrieveCertificatePassword(HttpRequestMessage request)
        {
            if (request.Headers.Authorization == null)
            {
                return null;
            }
            try
            {
                string basicParameter = Encoding.UTF8.GetString(Convert.FromBase64String(request.Headers.Authorization.Parameter));

                int index = basicParameter.IndexOf(':');

                if (index == -1)
                {
                    return null;
                }

                return Newtonsoft.Json.JsonConvert.DeserializeObject<CertificatePwdInformation>(basicParameter.Substring(index + 1));
            }
            catch (Exception ex)
            {
                // TODO: log what went wrong.
                return null;
            }
        }
    }

    internal class CertificatePwdInformation
    {
        public CertificatePwdInformation(string signingPwd, string decryptPwd)
        {
            SigningPassword = signingPwd;
            DecryptionPassword = decryptPwd;
        }

        public string SigningPassword { get; set; }
        public string DecryptionPassword { get; set; }
    }
}