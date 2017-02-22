using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using CommonService = Eu.EDelivery.AS4.Model.Common.Service;
using CoreService = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitServiceResolver"/>
    /// </summary>
    public class GivenSubmitServiceResolverFacts
    {
        
        public class GivenValidArguments : GivenSubmitServiceResolverFacts
        {
            [Fact]
            public void ThenResolverGetsServiceFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                submitMessage.Collaboration.Service = base.CreatePopulatedCommonService();
                submitMessage.PMode = new SendingProcessingMode();
                var resolver = new SubmitServiceResolver();
                // Act
                CoreService service = resolver.Resolve(submitMessage);
                // Assert
                Assert.Equal(submitMessage.Collaboration.Service.Value, service.Value);
                Assert.Equal(submitMessage.Collaboration.Service.Type, service.Type);
            }

            [Fact]
            public void ThenResolverGetsServiceFromPMode()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                var pmode = new SendingProcessingMode();
                pmode.MessagePackaging.CollaborationInfo = new CollaborationInfo();
                pmode.MessagePackaging.CollaborationInfo.Service = base.CreatePopulatedCoreService();
                submitMessage.PMode = pmode;
                var resolver = new SubmitServiceResolver();
                // Act
                CoreService service = resolver.Resolve(submitMessage);
                // Assert
                Assert.Equal(submitMessage.PMode.MessagePackaging.CollaborationInfo.Service, service);
            }

            [Fact]
            public void ThenResolverGetsDefaultService()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                submitMessage.PMode = new SendingProcessingMode();
                var resolver = new SubmitServiceResolver();
                // Act
                CoreService service = resolver.Resolve(submitMessage);
                // Assert
                Assert.Equal(Constants.Namespaces.TestService, service.Value);
            }
        }

        public class GivenInvalidArguments : GivenSubmitServiceResolverFacts
        {
            [Fact]
            public void ThenResolverFailsWhenOverrideIsNotAllowed()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                submitMessage.Collaboration.Service = base.CreatePopulatedCommonService();

                var pmode = new SendingProcessingMode();
                pmode.AllowOverride = false;
                pmode.MessagePackaging.CollaborationInfo = new CollaborationInfo();
                pmode.MessagePackaging.CollaborationInfo.Service = base.CreatePopulatedCoreService();
                submitMessage.PMode = pmode;
                var resolver = new SubmitServiceResolver();
                
                // Act / Assert
                Assert.Throws<AS4Exception>(() => resolver.Resolve(submitMessage));
            }
        }

        protected CommonService CreatePopulatedCommonService()
        {
            return new CommonService { Type = "submit-type", Value = "submit-value" };
        }

        protected CoreService CreatePopulatedCoreService()
        {
            return new CoreService
            {
                Value = "pmode-name",
                Type = "pmode-type"
            };
        }
    }
}