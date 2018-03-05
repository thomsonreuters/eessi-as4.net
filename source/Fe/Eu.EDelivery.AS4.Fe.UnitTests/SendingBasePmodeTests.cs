using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class SendingBasePmodeTests
    {
        [Fact]
        public void Contains_TlsConfiguration_When_Present_As_Json()
        {
            var certFindCriteria = new ClientCertificateReference
            {
                ClientCertificateFindType = X509FindType.FindBySubjectName,
                ClientCertificateFindValue = "subject"
            };
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode
                {
                    PushConfiguration = new PushConfiguration
                    {
                        TlsConfiguration =
                        {
                            IsEnabled = true,
                            ClientCertificateInformation = JObject.FromObject(certFindCriteria)
                        }
                    }
                }
            };

            Assert.IsType<ClientCertificateReference>(pmode.Pmode.PushConfiguration.TlsConfiguration.ClientCertificateInformation);
        }

        [Fact]
        public void IsDynamicDiscoveryEnabled_True_Will_Set_PushConfigurationNode_To_Null()
        {
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode
                {
                    PushConfiguration = new PushConfiguration()
                },
                IsDynamicDiscoveryEnabled = true
            };


            Assert.Null(pmode.Pmode.PushConfiguration);
        }

        [Fact]
        public void IsDynamicDiscoveryEnabled_False_When_PushConfiguration_Is_Not_Null()
        {
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode()
                {
                    PushConfiguration = new PushConfiguration()
                }
            };

            Assert.False(pmode.IsDynamicDiscoveryEnabled);
        }

        [Fact]
        public void IsDynamicDiscoveryEnabled_Is_True_Then_PushConfiguration_Is_Null()
        {
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode
                {
                    PushConfiguration = new PushConfiguration(),
                    DynamicDiscovery = new DynamicDiscoveryConfiguration()
                },
                IsDynamicDiscoveryEnabled = true
            };

            Assert.Null(pmode.Pmode.PushConfiguration);
            Assert.NotNull(pmode.Pmode.DynamicDiscovery);
        }
    }
}