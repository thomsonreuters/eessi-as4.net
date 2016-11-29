using System.IO;
using System.Linq;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.Tests
{
    public class RuntimeLoaderTest
    {
        public class Initialize
        {
            [Fact]
            public void Loads_All_Assemblies_Without_Throwing_Exception()
            {
                // Setup
                var loader = new Runtime.RuntimeLoader(Directory.GetCurrentDirectory());

                // Act
                loader.Initialize();
            }

            [Fact]
            public void Does_Not_Throw_Exception_When_Folder_Doesnt_Exist()
            {
                // Setup
                var loader = new Runtime.RuntimeLoader(@"c:\IDONTYEXIST");
                
                // Act
                loader.Initialize();
            }

            [Fact]
            public void Types_Should_Be_Loaded_From_The_Assemblies()
            {
                // Setup
                var loader = new Runtime.RuntimeLoader(Directory.GetCurrentDirectory());

                // Act
                loader.Initialize();

                // Assert
                Assert.True(loader.Receivers.Any());
                Assert.True(loader.Receivers.Any(type => type.Name == "FileReceiver"));
            }

            [Fact(Skip = "This is not the case anymore")]
            public void When_No_InfoAttribute_Present_Property_Info_Should_Be_Used()
            {
                // Setup
                var loader = new Runtime.RuntimeLoader(Path.Combine(Directory.GetCurrentDirectory(), "bin/debug/netcoreapp1.0/"));

                var types = loader.LoadTypesFromAssemblies();

                // Act
                var result = loader.LoadImplementationsForType(types, "Eu.EDelivery.AS4.Fe.Tests.TestData.ITestReceiver");

                // Assert
                var first = result.First();
                Assert.True(first.Name == "TestReceiver");
                Assert.True(first.Properties.Any(x => x.FriendlyName == "Name"));
            }

            [Fact]
            public void When_InfoAttribute_Or_DescriptionAttribute_Is_Present_They_Should_Be_Used()
            {
                // Setup
                var loader = new Runtime.RuntimeLoader(Path.Combine(Directory.GetCurrentDirectory(), "bin/debug/netcoreapp1.0/"));

                var types = loader.LoadTypesFromAssemblies();

                // Act
                var result = loader.LoadImplementationsForType(types, "Eu.EDelivery.AS4.Fe.Tests.TestData.ITestReceiver");

                // Assert
                var first = result.First();
                Assert.True(first.Name == "TestReceiver");
                Assert.True(first.TechnicalName == "Eu.EDelivery.AS4.Fe.Tests.TestData.TestReceiver, Eu.EDelivery.AS4.Fe.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                var info = first.Properties.FirstOrDefault(prop => prop.FriendlyName == "FRIENDLYNAME");
                Assert.NotNull(info);
                Assert.True(info.Regex == "REGEX");
                Assert.True(info.Type == "TYPE");
                Assert.True(info.Description == "DESCRIPTION");
            }
        }
    }
}