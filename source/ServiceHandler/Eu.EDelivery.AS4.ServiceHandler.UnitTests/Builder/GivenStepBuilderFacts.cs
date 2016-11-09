using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.ServiceHandler.Builder;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Xunit;

namespace Eu.EDelivery.AS4.ServiceHandler.UnitTests.Builder
{
    /// <summary>
    /// Testing <see cref="StepBuilder"/>
    /// </summary>
    public class GivenStepBuilderFacts
    {
        public class GivenValidArguments : GivenStepBuilderFacts
        {
            [Fact]
            public void ThenBuilderCreatesValidStep()
            {
                // Arrange
                Model.Internal.Steps settingSteps = CreateDefaultSettingSteps();
                // Act
                IStep step = new StepBuilder().SetSettings(settingSteps).Build();
                // Assert
                Assert.IsType<CompositeStep>(step);
            }

            private Model.Internal.Steps CreateDefaultSettingSteps()
            {
                return new Model.Internal.Steps()
                {
                    Step = new[]
                    {
                        new Step {Type = typeof(EncryptAS4MessageStep).AssemblyQualifiedName}
                    }
                };
            }
        }
    }
}