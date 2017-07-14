using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class SendingBasePmodeTests
    {
        [Fact]
        public void IsPushConfigurationEnabled_False_Will_Set_PushConfigurationNode_To_Null()
        {
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode
                {
                    PushConfiguration = new PushConfiguration()
                },
                IsPushConfigurationEnabled = false
            };


            Assert.Null(pmode.Pmode.PushConfiguration);
        }

        [Fact]
        public void IsPushConfigurationEnabled_Is_False_When_PushConfiguration_Is_Null()
        {
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode
                {
                }
            };

            Assert.False(pmode.IsPushConfigurationEnabled);
        }

        [Fact]
        public void IsPushConfigurationEnabled_Is_True_Then_DynamicDiscovery_Is_Null()
        {
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode()
                {
                    DynamicDiscovery = new DynamicDiscoveryConfiguration()
                },
                IsPushConfigurationEnabled = true
            };

            Assert.Null(pmode.Pmode.DynamicDiscovery);
        }

        [Fact]
        public void IsPushConfigurationEnabled_Is_True_When_PushConfiguration_Is_Null()
        {
            var pmode = new SendingBasePmode
            {
                Pmode = new SendingProcessingMode
                {
                    PushConfiguration = new PushConfiguration()
                }
            };

            Assert.True(pmode.IsPushConfigurationEnabled);
        }
    }
}