using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
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

                // Act
                CoreService service = SubmitServiceResolver.ResolveService(submitMessage);

                // Assert
               Assert.Equal(CoreService.TestService, service);
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
                        CollaborationInfo = new AS4.Model.PMode.CollaborationInfo {Service = CreatePopulatedCoreService()}
                    }
                };
                submitMessage.PMode = pmode;

                // Act
                CoreService actual = SubmitServiceResolver.ResolveService(submitMessage);

                // Assert
                Service expected = submitMessage.PMode.MessagePackaging.CollaborationInfo.Service;
                Assert.Equal(expected.Value, actual.Value);
                Assert.Equal(Maybe.Just(expected.Type), actual.Type);
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

                // Act
                CoreService service = SubmitServiceResolver.ResolveService(submitMessage);

                // Assert
                Assert.Equal(submitMessage.Collaboration.Service.Value, service.Value);
                Assert.Equal(Maybe.Just(submitMessage.Collaboration.Service.Type), service.Type);
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


                // Act / Assert
                Assert.ThrowsAny<Exception>(() => SubmitServiceResolver.ResolveService(submitMessage));
            }
        }

        protected CommonService CreatePopulatedCommonService()
        {
            return new CommonService {Type = "submit-type", Value = "submit-value"};
        }

        protected Service CreatePopulatedCoreService()
        {
            return new Service {Value = "pmode-name", Type = "pmode-type"};
        }
    }
}