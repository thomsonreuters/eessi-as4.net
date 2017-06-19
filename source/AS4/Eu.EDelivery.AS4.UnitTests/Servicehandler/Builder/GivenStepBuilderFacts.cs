using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Common;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Steps;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Servicehandler.Builder
{
    /// <summary>
    /// Testing <see cref="AS4.Steps.StepBuilder" />
    /// </summary>
    public class GivenStepBuilderFacts
    {
        public class GivenValidStepSettings : GivenStepBuilderFacts
        {

            [Fact]
            public void BuilderCreatesExpectedAmountOfSteps()
            {
                // Arrange
                int expectedCount = new Random().Next(0, 10);
                Type expectedType = typeof(SinkStep);

                // Act
                IEnumerable<IStep> steps = 
                    StepBuilder.FromSettings(CreatePipelineSteps(expectedCount, expectedType)).BuildSteps();

                // Assert
                Assert.Equal(expectedCount, steps.Count());
                Assert.All(steps, s => Assert.Equal(expectedType, s.GetType()));
            }

            private static AS4.Model.Internal.Steps CreatePipelineSteps(int amount, Type type)
            {
                var stubStep = new Step {Type = type.AssemblyQualifiedName};

                return new AS4.Model.Internal.Steps {Step = Enumerable.Repeat(stubStep, amount).ToArray()};
            }

            [Fact]
            public void ThenBuilderCreatesComplexStep()
            {
                // Arrange
                AS4.Model.Internal.Steps settings = CreateComplexStepSettings();

                // Act
                IStep step = StepBuilder.FromSettings(settings).Build();

                // Assert
                Assert.NotNull(step);
                Assert.IsType<CompositeStep>(step);
            }

            [Fact]
            public void ThenBuilderCreatesDecoratedCompositeStep()
            {
                // Arrange
                AS4.Model.Internal.Steps settings = CreateDecoratedCompositeStepSettings();

                // Act
                IStep step = StepBuilder.FromSettings(settings).Build();

                // Assert
                Assert.NotNull(step);
                Assert.IsType<OutExceptionStepDecorator>(step);
            }

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

            private static AS4.Model.Internal.Steps CreateDefaultSettingSteps()
            {
                return new AS4.Model.Internal.Steps
                {
                    Step = new[] {new Step {Type = typeof(EncryptAS4MessageStep).AssemblyQualifiedName}}
                };
            }

            private static AS4.Model.Internal.Steps CreateDecoratedCompositeStepSettings()
            {
                return new AS4.Model.Internal.Steps
                {
                    Decorator = typeof(OutExceptionStepDecorator).AssemblyQualifiedName,
                    Step =
                        new[]
                        {
                            new Step {Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName},
                            new Step {Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName}
                        }
                };
            }

            private static AS4.Model.Internal.Steps CreateComplexStepSettings()
            {
                return new AS4.Model.Internal.Steps
                {
                    Decorator = typeof(ReceiveExceptionStepDecorator).AssemblyQualifiedName,
                    Step =
                        new[]
                        {
                            new Step {Type = typeof(DeterminePModesStep).AssemblyQualifiedName},
                            new Step {Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName},
                            new Step {Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName},
                            new Step {Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName},
                            new Step {Type = typeof(SaveReceivedMessageStep).AssemblyQualifiedName},
                            new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName},
                            new Step {Type = typeof(StoreAS4ReceiptStep).AssemblyQualifiedName},
                            new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                            new Step {Type = typeof(SendAS4MessageStep).AssemblyQualifiedName},
                            new Step {UnDecorated = true, Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName},
                            new Step {UnDecorated = true, Type = typeof(SignAS4MessageStep).AssemblyQualifiedName}
                        }
                };
            }
        }

        public class GivenValidConditionalStepConfig
        {
            [Fact]
            public void BuildConditionalStep_AsList()
            {
                // Arrange
                ConditionalStepConfig config = CreateSimpleConditationStepConfig();

                // Act
                IEnumerable<IStep> step = StepBuilder.FromConditionalConfig(config).BuildSteps();

                // Assert
                IStep first = step.First();
                AssertConditionalStep(first);
            }

            [Fact]
            public void BuildConditionanStep_AsInstance()
            {
                // Arrange
                ConditionalStepConfig config = CreateSimpleConditationStepConfig();

                // Act
                IStep step = StepBuilder.FromConditionalConfig(config).Build();

                // Assert
                AssertConditionalStep(step);
            }

            private static void AssertConditionalStep(IStep step)
            {
                Assert.NotNull(step);
                Assert.IsType<ConditionalStep>(step);
            }

            private static ConditionalStepConfig CreateSimpleConditationStepConfig()
            {
                var thenStep = new AS4.Model.Internal.Steps
                {
                    Step = new[] {new Step {Type = typeof(DeterminePModesStep).AssemblyQualifiedName}}
                };

                var elseStep = new AS4.Model.Internal.Steps
                {
                    Step = new[] {new Step {Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName}}
                };

                return new ConditionalStepConfig(null, thenStep, elseStep);
            }
        }
    }
}