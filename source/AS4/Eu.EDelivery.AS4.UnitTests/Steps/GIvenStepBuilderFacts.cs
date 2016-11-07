using System.Collections.Generic;
using System.Reflection;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Common;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Steps.Receive;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps
{
    /// <summary>
    /// Testing the <see cref="StepBuilder" />
    /// </summary>
    public class 
        GivenStepBuilderFacts
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        private readonly StepBuilder _builder;

        public GivenStepBuilderFacts()
        {
            this._builder = new StepBuilder();
        }

        /// <summary>
        /// Testing the Builder with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenStepBuilderFacts
        {
            [Fact]
            public void ThenBuildSucceedsWithSendSteps()
            {
                // Act
                IStep step = this._builder.BuildSendStep();
                // Assert
                Assert.IsType(typeof(OutExceptionStepDecorator), step);
                IList<IStep> steps = base.GetChildSendSteps(step);
                Assert.NotNull(steps);
                Assert.Equal(5, steps.Count);
            }

            [Fact]
            public void ThenBuildSucceedsWithSubmitSteps()
            {
                // Act
                IStep step = this._builder.BuildSubmitStep();
                // Assert
                IList<IStep> steps = base.GetChildSendSteps(step);
                Assert.NotNull(steps);
                Assert.Equal(4, steps.Count);
            }

            [Fact]
            public void ThenBuildSucceedsWithReceiveSteps()
            {
                // Act
                IStep step = this._builder.BuildReceiveStep();
                // Assert
                Assert.IsType(typeof(ReceiveExceptionStepDecorator), step);
                IList<IStep> steps = base.GetChildReceiveSteps(step);
                Assert.NotNull(steps);
                Assert.Equal(8, steps.Count);
            }

            [Fact]
            public void ThenBuildSucceedsWithDeliverSteps()
            {
                // Act
                IStep step = this._builder.BuildDeliverStep();
                // Assert
                IList<IStep> steps = base.GetChildDeliverSteps(step);
                Assert.NotNull(steps);
                Assert.Equal(4, steps.Count);
            }
        }

        /// <summary>
        /// Testing the Builder with invalid arguments
        /// </summary>
        public class GivenInvalidArguments : GivenStepBuilderFacts
        {
            [Fact]
            public void ThenWithOptionsBuildDoesNotContainsUpdateDataStore()
            {
                // Act
                IStep step = this._builder
                    .WithOptions(StepOptions.UseDefaults)
                    .BuildSendStep();
                // Assert
                AssertSendStepWithoutDatastoreSteps(step);
            }

            private void AssertSendStepWithoutDatastoreSteps(IStep step)
            {
                FieldInfo decoratorStepField = typeof(OutExceptionStepDecorator).GetField("_step", BindingFlags);
                FieldInfo childStepsField = typeof(CompositeStep).GetField("_steps", BindingFlags);

                var decoratorStep = decoratorStepField.GetValue(step) as IStep;
                var steps = childStepsField.GetValue(decoratorStep) as IList<IStep>;

                Assert.Equal(3, steps.Count);
            }
        }

        protected IList<IStep> GetChildSendSteps(IStep step)
        {
            FieldInfo compositeStepField = typeof(OutExceptionStepDecorator).GetField("_step", BindingFlags);
            FieldInfo childStepsField = typeof(CompositeStep).GetField("_steps", BindingFlags);

            var compositeStep = compositeStepField.GetValue(step) as IStep;
            return childStepsField.GetValue(compositeStep) as IList<IStep>;
        }

        protected IList<IStep> GetChildReceiveSteps(IStep step)
        {
            FieldInfo compositeStepField = typeof(ReceiveExceptionStepDecorator).GetField("_step", BindingFlags);
            FieldInfo childStepsField = typeof(CompositeStep).GetField("_steps", BindingFlags);

            var compositeStep = compositeStepField.GetValue(step) as IStep;
            return childStepsField.GetValue(compositeStep) as IList<IStep>;
        }

        protected IList<IStep> GetChildDeliverSteps(IStep step)
        {
            FieldInfo compositeStepField = typeof(InExceptionStepDecorator).GetField("_step", BindingFlags);
            FieldInfo childStepsField = typeof(CompositeStep).GetField("_steps", BindingFlags);

            var compositeStep = compositeStepField.GetValue(step) as IStep;
            return childStepsField.GetValue(compositeStep) as IList<IStep>;
        }
    }
}