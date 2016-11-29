using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public class RuntimeLoader : IRuntimeLoader
    {
        private const string INFOATTRIBUTE = "InfoAttribute";
        private const string DESCRIPTIONATTRIBUTE = "DescriptionAttribute";
        private readonly string folder;
        public IEnumerable<ItemType> Receivers { get; private set; }
        public IEnumerable<ItemType> Steps { get; private set; }
        public IEnumerable<ItemType> Transformers { get; private set; }

        public RuntimeLoader(string folder)
        {
            this.folder = folder;
        }

        public IRuntimeLoader Initialize()
        {
            if (!Directory.Exists(folder)) return this;

            var types = LoadTypesFromAssemblies();
            Receivers = LoadImplementationsForType(types, "Eu.EDelivery.AS4.Receivers.IReceiver");
            Steps = LoadImplementationsForType(types, "Eu.EDelivery.AS4.Steps.IStep");
            Transformers = LoadImplementationsForType(types, "Eu.EDelivery.AS4.Transformers.ITransformer");
            return this;
        }
        public List<TypeDefinition> LoadTypesFromAssemblies()
        {
            return Directory
                .GetFiles(folder)
                .Where(file => Path.GetExtension(file) == ".dll")
                .SelectMany(file => AssemblyDefinition.ReadAssembly(file).MainModule.Types)
                .ToList();
        }


        public IEnumerable<ItemType> LoadImplementationsForType(List<TypeDefinition> types, string type)
        {
            var implementations = types.Where(x => x.Interfaces.Any(iface => iface.InterfaceType.FullName == type));
            var itemTypes = implementations.Select(itemType => BuildItemType(itemType, BuildProperties(itemType.Properties)));
            return itemTypes.Where(x => x != null);
        }

        private ItemType BuildItemType(TypeDefinition itemType, IEnumerable<Property> properties)
        {
            return new ItemType()
            {
                Name = itemType.Name,
                Properties = properties
            };
        }

        private IEnumerable<Property> BuildProperties(Collection<PropertyDefinition> properties)
        {
            var runtimeProperties = properties
                //.Where(prop => prop.GetMethod.IsPublic)
                .Select(prop =>
                {
                    var customAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == INFOATTRIBUTE);
                    var descriptionAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == DESCRIPTIONATTRIBUTE);
                    if (customAttr == null)
                    {
                        return null;
                        // No CustomAttribute found, use defaults which mean using the property info
                        //return new Property()
                        //{
                        //    FriendlyName = prop.Name,
                        //    Type = prop.PropertyType.Name
                        //};
                    }
                    else
                    {
                        var arguments = customAttr.ConstructorArguments;
                        var descriptionArgs = descriptionAttr?.ConstructorArguments;
                        var count = arguments.Count;
                        return new Property()
                        {
                            FriendlyName = arguments[0].Value as string,
                            Regex = count > 2 ? arguments[1].Value as string : "",
                            Type = count > 1 ? arguments[2].Value as string : "",
                            Description = descriptionArgs?.Count > 0 ? descriptionArgs[0].Value as string : ""
                        };
                    }
                });

            return runtimeProperties.Where(x => x != null);
        }
    }
}