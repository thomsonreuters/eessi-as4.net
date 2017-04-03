using System;
using Eu.EDelivery.AS4.Builders;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Servicehandler.Builder
{
    /// <summary>
    /// Testing <see cref="GenericTypeBuilder"/>
    /// </summary>
    public class GivenGenericTypeBuilderFacts
    {
        public class GivenValidArguments : GivenGenericTypeBuilderFacts
        {
            [Fact]
            public void ThenBuilderCreatesValidType()
            {
                // Arrange
                string typeString = typeof(object).FullName;
                // Act
                var instance = GenericTypeBuilder.FromType(typeString).Build<object>();
                // Assert
                Assert.NotNull(instance);
                Assert.IsType<object>(instance);
            }
        }

        public class GivenInValidArguments : GivenGenericTypeBuilderFacts
        {
            [Fact]
            public void ThenBuilderFailsToCreateTypeForAssemblyName()
            {
                // Arrange
                const string typeString =
                    "Eu.EDelivery.AS4.Transformers.InvalidTransformer, Eu.EDelivery.AS4.Transformers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                // Act
                Assert.Throws<TypeLoadException>(
                    () => GenericTypeBuilder.FromType(typeString));
            }
        }
    }
}