using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Steps.Submit;
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

            private static Step[] CreatePipelineSteps(int amount, Type type)
            {
                var stubStep = new Step { Type = type.AssemblyQualifiedName };

                return Enumerable.Repeat(stubStep, amount).ToArray();
            }

            [Fact]
            public void ThenBuilderCreatesValidStep()
            {
                // Arrange
                Step[] settingSteps = CreateDefaultSettingSteps();

                // Act
                IStep step = StepBuilder.FromSettings(settingSteps).BuildAsSingleStep();

                // Assert
                Assert.IsType<CompositeStep>(step);
            }

            private static Step[] CreateDefaultSettingSteps()
            {
                return new[] { new Step { Type = typeof(EncryptAS4MessageStep).AssemblyQualifiedName } };
            }
        }

        public class GivenValidConditionalStepConfig
        {
            [Fact]
            public void BuildConditionalStep_AsList()
            {
                // Arrange
                ConditionalStepConfig config = CreateSimpleConditationalStepConfig();

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
                ConditionalStepConfig config = CreateSimpleConditationalStepConfig();

                // Act
                IStep step = StepBuilder.FromConditionalConfig(config).BuildAsSingleStep();

                // Assert
                AssertConditionalStep(step);
            }

            private static void AssertConditionalStep(IStep step)
            {
                Assert.NotNull(step);
                Assert.IsType<ConditionalStep>(step);
            }

            private static ConditionalStepConfig CreateSimpleConditationalStepConfig()
            {
                var thenStep = new[] { new Step { Type = typeof(DeterminePModesStep).AssemblyQualifiedName } };
                var elseStep = new[] { new Step { Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName } };

                return new ConditionalStepConfig(null, thenStep, elseStep);
            }
        }

        public class GivenInvalidConfigurableStepConfig : GivenStepBuilderFacts
        {
            [Fact]
            public void NonConfigurableStepWithSettingsThrowsConfigurationException()
            {
                StepConfiguration config = CreateInvalidConfigurableStepConfig();

                Assert.Throws<ConfigurationErrorsException>(() => StepBuilder.FromSettings(config.NormalPipeline).BuildAsSingleStep());
            }

            private static StepConfiguration CreateInvalidConfigurableStepConfig()
            {
                var step = new Step
                {
                    Type = typeof(DynamicDiscoveryStep).AssemblyQualifiedName,
                    Setting = new[] { new Setting("SmpProfile", "someValue"), }
                };

                return new StepConfiguration() { NormalPipeline = new[] { step } };
            }
        }
    }
}