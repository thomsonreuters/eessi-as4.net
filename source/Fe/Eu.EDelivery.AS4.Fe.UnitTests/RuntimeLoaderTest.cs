using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.Fe.Runtime;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Transformers;
using Microsoft.Extensions.Options;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class RuntimeLoaderTest
    {
        private RuntimeLoader loader;
        private List<TypeDefinition> types;

        private RuntimeLoaderTest Setup()
        {
            var options = Substitute.For<IOptions<ApplicationSettings>>();
            options.Value.Returns(new ApplicationSettings()
            {
                Runtime = Directory.GetCurrentDirectory()
            });
            loader = new RuntimeLoader(options);
            types = loader.LoadTypesFromAssemblies();
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
            public void Interfaces_Should_Not_Be_In_The_List()
            {
                Setup();

                var istep = typeof(IConfigStep).Name;

                Assert.True(loader.Steps.All(y => !y.Name.Contains(istep)), "No interfaces types should be in the list.");
            }

            [Fact]
            public void Abstract_Classes_Should_Not_Be_In_The_List()
            {
                Setup();

                var abstractClass = typeof(MinderNotifyMessageTransformer).Name;

                Assert.True(loader.Transformers.All(y => !y.Name.Contains(abstractClass)), "No abstract types should be in the list.");
            }

            [Fact]
            public void Types_Decorated_With_NoUi_Attribute_Should_Not_Be_In_The_List()
            {
                Setup();

                var conditionalstep = typeof(ConditionalStep).Name;

                Assert.True(loader.Steps.All(y => !y.Name.Contains(conditionalstep)), "Types decorates with the NoUiAttribute should not be in the list.");
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
                Assert.True(first.TechnicalName.Contains("TestReceiver"));

                var info = first.Properties.FirstOrDefault(prop => prop.FriendlyName == "FRIENDLYNAME");
                Assert.NotNull(info);
                Assert.True(info.Regex == "REGEX");
                Assert.True(info.Type == "TYPE");
                Assert.True(info.Description == "DESCRIPTION");
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

        public class FlattenRuntimeToJson : RuntimeLoaderTest
        {
            [Fact]
            public void Object_Properties_Should_Be_Flattened_In_Json()
            {
                // Setup
                Setup();

                var result = loader.LoadImplementationsForType(types, "Eu.EDelivery.AS4.Model.PMode.IPMode");

                var jsonResult = JsonConvert.SerializeObject(result.First(x => x.Name == "SendingProcessingMode"), Formatting.Indented, new FlattenRuntimeToJsonConverter());

                // Assert
                var json = JObject.Parse(jsonResult);
                Assert.NotNull(json.Properties().FirstOrDefault(prop => prop.Name == "pullconfiguration"));
                Assert.NotNull(json.Properties().FirstOrDefault(prop => prop.Name == "security"));
                Assert.NotNull(json.Properties().FirstOrDefault(prop => prop.Name == "signing"));
            }
        }
    }
}