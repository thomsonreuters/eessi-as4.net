using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using CommonService = Eu.EDelivery.AS4.Model.Common.Service;
using CoreService = Eu.EDelivery.AS4.Model.Core.Service;
using Exception = System.Exception;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitServiceResolver" />
    /// </summary>
    public class GivenSubmitServiceResolverFacts
    {
        public class GivenValidArguments : GivenSubmitServiceResolverFacts
        {
            [Fact]
            public void ThenResolverGetsDefaultService()
            {
                // Arrange
                var submitMessage = new SubmitMessage {PMode = new SendingProcessingMode()};
                var resolver = SubmitServiceResolver.Default;

                // Act
                CoreService service = resolver.Resolve(submitMessage);

                // Assert
                Assert.True(string.IsNullOrWhiteSpace(service.Value));
            }

            [Fact]
            public void ThenResolverGetsServiceFromPMode()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                var pmode = new SendingProcessingMode
                {
                    MessagePackaging =
                    {
                        CollaborationInfo = new CollaborationInfo {Service = CreatePopulatedCoreService()}
                    }
                };
                submitMessage.PMode = pmode;
                var resolver = SubmitServiceResolver.Default;

                // Act
                CoreService service = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(submitMessage.PMode.MessagePackaging.CollaborationInfo.Service, service);
            }

            [Fact]
            public void ThenResolverGetsServiceFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    Collaboration = {Service = CreatePopulatedCommonService()},
                    PMode = new SendingProcessingMode()
                };
                var resolver = SubmitServiceResolver.Default;

                // Act
                CoreService service = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(submitMessage.Collaboration.Service.Value, service.Value);
                Assert.Equal(submitMessage.Collaboration.Service.Type, service.Type);
            }
        }

        public class GivenInvalidArguments : GivenSubmitServiceResolverFacts
        {
            [Fact]
            public void ThenResolverFailsWhenOverrideIsNotAllowed()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    Collaboration = {Service = CreatePopulatedCommonService()},
                    PMode =
                        new SendingProcessingMode
                        {
                            AllowOverride = false,
                            MessagePackaging =
                            {
                                CollaborationInfo = new CollaborationInfo {Service = CreatePopulatedCoreService()}
                            }
                        }
                };

                var resolver = SubmitServiceResolver.Default;

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => resolver.Resolve(submitMessage));
            }
        }

        protected CommonService CreatePopulatedCommonService()
        {
            return new CommonService {Type = "submit-type", Value = "submit-value"};
        }

        protected CoreService CreatePopulatedCoreService()
        {
            return new CoreService {Value = "pmode-name", Type = "pmode-type"};
        }
    }
}