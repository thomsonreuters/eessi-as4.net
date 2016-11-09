using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.ServiceHandler.Builder;
using Xunit;

namespace Eu.EDelivery.AS4.ServiceHandler.UnitTests.Builder
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
                var instance = new GenericTypeBuilder().SetType(typeString).Build<object>();
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
                Assert.Throws<AS4Exception>(
                    () => new GenericTypeBuilder().SetType(typeString));
            }
        }
    }
}