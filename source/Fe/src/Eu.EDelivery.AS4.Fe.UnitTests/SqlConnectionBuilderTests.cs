using System;
using Eu.EDelivery.AS4.Fe.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class SqlConnectionBuilderTests
    {
        [Fact]
        public void Throws_Exception_When_Provider_Is_Not_Recognized()
        {
            var result = Assert.Throws<Exception>(() => SqlConnectionBuilder.Build("DONTEXIST", string.Empty, null));
            Assert.True(result.Message == "No provider found for DONTEXIST");
        }

        [Theory]
        [InlineData("sqlite")]
        [InlineData("sqlserver")]
        public void InitializesConnection(string provider)
        {
            var test = new DbContextOptionsBuilder();
            SqlConnectionBuilder.Build(provider, "test", test);
            Assert.True(test.IsConfigured);
        }
    }
}
