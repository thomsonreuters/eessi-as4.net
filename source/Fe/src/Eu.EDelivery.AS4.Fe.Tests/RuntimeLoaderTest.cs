using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.Fe.Runtime;
using Mono.Cecil;
using Newtonsoft.Json;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.Tests
{
    public class RuntimeLoaderTest
    {
        private RuntimeLoader loader;
        private List<TypeDefinition> types;

        private RuntimeLoaderTest Setup()
        {
            loader = new RuntimeLoader(Directory.GetCurrentDirectory());
            types = loader.LoadTypesFromAssemblies();
            loader.Initialize();
            return this;
        }

        public class Initialize : RuntimeLoaderTest
        {
            [Fact]
            public void Types_Should_Be_Loaded_From_The_Assemblies()
            {
                // Setup
                Setup();

                // Assert
                Assert.True(loader.Receivers.Any());
                Assert.True(loader.Receivers.Any(type => type.Name == "File receiver"));
            }

            [Fact]
            public void When_InfoAttribute_And_DescriptionAttribute_Is_Present_They_Should_Be_Used()
            {
                // Setup
                Setup();
                var result = loader.LoadImplementationsForType(types, "Eu.EDelivery.AS4.Fe.Tests.TestData.ITestReceiver");

                // Assert
                var first = result.First();
                Assert.True(first.Name == "Test receiver");
                Assert.True(first.TechnicalName == "Eu.EDelivery.AS4.Fe.Tests.TestData.TestReceiver, Eu.EDelivery.AS4.Fe.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                var info = first.Properties.FirstOrDefault(prop => prop.FriendlyName == "FRIENDLYNAME");
                Assert.NotNull(info);
                Assert.True(info.Regex == "REGEX");
                Assert.True(info.Type == "TYPE");
                Assert.True(info.Description == "DESCRIPTION");
            }

            [Fact(Skip = "This is not the case anymore")]
            public void When_No_InfoAttribute_Present_Property_Info_Should_Be_Used()
            {
                // Setup
                var loader = new RuntimeLoader(Directory.GetCurrentDirectory());

                var types = loader.LoadTypesFromAssemblies();

                // Act
                var result = loader.LoadImplementationsForType(types, "Eu.EDelivery.AS4.Fe.Tests.TestData.ITestReceiver");

                // Assert
                var first = result.First();
                Assert.True(first.Name == "TestReceiver");
                Assert.True(first.Properties.Any(x => x.FriendlyName == "Name"));
            }

            [Fact]
            public void When_Only_DescriptionAttribute_Is_Present_It_Should_Be_Used()
            {
                // Setup
                Setup();

                // Act
                var result = loader.LoadImplementationsForType(types, "Eu.EDelivery.AS4.Fe.Tests.TestData.ITestReceiver");
                var onlywithDescription = result.First(test => test.Name.ToLower().Contains("testreceiverwithonlydescription"));

                // Assert
                Assert.True(onlywithDescription.Description == "TestReceiverWithOnlyDescription");
                Assert.True(onlywithDescription.Properties.First().Description == "Name");
            }
        }

        public class FlattenRuntimeToJson
        {
            [Fact]
            public void Object_Properties_Should_Be_Flattened_In_Json()
            {
                // Setup
                var loader = new RuntimeLoader(Directory.GetCurrentDirectory());
                var types = loader.LoadTypesFromAssemblies();

                var result = loader.LoadImplementationsForType(types, "Eu.EDelivery.AS4.Model.PMode.IPMode");

                var expected = File.ReadAllText(@"metadata_sendingprocessingmode_flatten.json");
                var jsonResult = JsonConvert.SerializeObject(result.First(x => x.Name == "SendingProcessingMode"), Formatting.Indented, new FlattenRuntimeToJsonConverter());

                // Assert
                Assert.True(jsonResult == expected);
            }
        }
    }
}