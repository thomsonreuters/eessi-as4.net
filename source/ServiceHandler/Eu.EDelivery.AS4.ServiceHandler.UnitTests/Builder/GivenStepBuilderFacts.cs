using System;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Common;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Xunit;
using StepBuilder = Eu.EDelivery.AS4.Steps.StepBuilder;

namespace Eu.EDelivery.AS4.ServiceHandler.UnitTests.Builder
{
    /// <summary>
    /// Testing <see cref="AS4.Steps.StepBuilder"/>
    /// </summary>
    public class GivenStepBuilderFacts
    {
        public class GivenValidStepSettings : GivenStepBuilderFacts
        {
            [Fact]
            public void ThenBuilderCreatesValidStep()
            {
                // Arrange
                AS4.Model.Internal.Steps settingSteps = CreateDefaultSettingSteps();
                // Act
                IStep step = StepBuilder.FromSettings(settingSteps).Build();
                // Assert
                Assert.IsType<CompositeStep>(step);
            }

            [Fact]
            public void ThenBuilderCreatesDecoratedCompositeStep()
            {
                var settings = CreateDecoratedCompositeStepSettings();

                var step = StepBuilder.FromSettings(settings).Build();

                Assert.NotNull(step);
                Assert.IsType<OutExceptionStepDecorator>(step);
            }

            [Fact]
            public void ThenBuilderCreatesComplexStep()
            {
                var settings = CreateComplexStepSettings();

                var step = StepBuilder.FromSettings(settings).Build();

                Assert.NotNull(step);
                Assert.IsType<CompositeStep>(step);
            }

            private static AS4.Model.Internal.Steps CreateDefaultSettingSteps()
            {
                return new AS4.Model.Internal.Steps()
                {
                    Step = new[]
                    {
                        new Step {Type = typeof(EncryptAS4MessageStep).AssemblyQualifiedName}
                    }
                };
            }

            private static AS4.Model.Internal.Steps CreateDecoratedCompositeStepSettings()
            {
                return new AS4.Model.Internal.Steps()
                {
                    Decorator = typeof(OutExceptionStepDecorator).AssemblyQualifiedName,
                    Step = new[]
                    {
                        new Step() { Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName},
                        new Step() { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName}
                    }
                };
            }

            private static AS4.Model.Internal.Steps CreateComplexStepSettings()
            {
                return new Model.Internal.Steps()
                {
                    Decorator = typeof(ReceiveExceptionStepDecorator).AssemblyQualifiedName,
                    Step = new Step[]
                    {
                        new Step { Type = typeof(DeterminePModesStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName },
                        new Step { Type = typeof(ReceiveUpdateDatastoreStep).AssemblyQualifiedName},
                        new Step { Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName },
                        new Step { Type = typeof(StoreAS4ReiptStep).AssemblyQualifiedName},
                        new Step { Type = typeof(SignAS4MessageStep).AssemblyQualifiedName },
                        new Step { Type = typeof(SendAS4MessageStep).AssemblyQualifiedName },
                        new Step { UnDecorated = true,Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName },
                        new Step { UnDecorated = true, Type=typeof(SignAS4MessageStep).AssemblyQualifiedName},
                    }
                };
            }
        }

        public class GivenValidConditionalStepConfig
        {
            [Fact]
            public void ThenBuilderCreatesConditionalStep()
            {
                var config = CreateSimpleConditationStepConfig();

                var step = StepBuilder.FromConditionalConfig(config).Build();

                Assert.NotNull(step);
                Assert.IsType<ConditionalStep>(step);
            }

            private static ConditionalStepConfig CreateSimpleConditationStepConfig()
            {
                var thenStep = new Model.Internal.Steps()
                {
                    Step = new Step[]
                    {
                        new Step() { Type = typeof(DeterminePModesStep).AssemblyQualifiedName }
                    }
                };

                var elseStep = new Model.Internal.Steps()
                {
                    Step = new Step[]
                    {
                        new Step() { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName }
                    }
                };

                return new ConditionalStepConfig(x => String.IsNullOrEmpty(x.Prefix), thenStep, elseStep);
            }
        }
    }
}