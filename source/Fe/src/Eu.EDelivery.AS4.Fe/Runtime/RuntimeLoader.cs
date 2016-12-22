using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public class RuntimeLoader : IRuntimeLoader
    {
        private const string Infoattribute = "InfoAttribute";
        private const string Descriptionattribute = "DescriptionAttribute";
        private readonly string folder;
        public IEnumerable<ItemType> Receivers { get; private set; }
        public IEnumerable<ItemType> Steps { get; private set; }
        public IEnumerable<ItemType> Transformers { get; private set; }
        public IEnumerable<ItemType> CertificateRepositories { get; private set; }
        public IEnumerable<ItemType> DeliverSenders { get; private set; }
        public IEnumerable<ItemType> ReceivingPmode { get; private set; }

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
            CertificateRepositories = LoadImplementationsForType(types, "Eu.EDelivery.AS4.Repositories.ICertificateRepository");
            DeliverSenders = LoadImplementationsForType(types, "Eu.EDelivery.AS4.Strategies.Sender.IDeliverSender");
            ReceivingPmode = LoadImplementationsForType(types, "Eu.EDelivery.AS4.Model.PMode.IPMode");

            return this;
        }
        public List<TypeDefinition> LoadTypesFromAssemblies()
        {
            return Directory
                .GetFiles(folder)
                .Where(file => Path.GetExtension(file) == ".dll")
                .Where(path =>
                {
                    // BadImageFormatException is being thrown on the following dlls since the code base switched to net 461.
                    // Since these dlls are not needed by the FE they're filtered out to avoid this exception.
                    // TODO: Probably fixed when AS4 targets dotnet core.
                    var file = Path.GetFileName(path) ?? string.Empty;
                    return file != "libuv.dll" && !file.StartsWith("Microsoft") && !file.StartsWith("System") && file != "sqlite3.dll";
                })
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
            // Get class info attribute
            var infoAttribute = itemType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Infoattribute);

            return new ItemType()
            {
                Name = infoAttribute == null ? itemType.Name : infoAttribute.ConstructorArguments[0].Value as string,
                TechnicalName = $"{itemType.FullName}, {itemType.Module.Assembly.FullName}",
                Properties = properties
            };
        }

        private IEnumerable<Property> BuildProperties(Collection<PropertyDefinition> properties)
        {
            foreach (var prop in properties)
            {
                var customAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Infoattribute);
                var descriptionAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Descriptionattribute);
                Property property = null;
                if (customAttr == null)
                {
                    property = new Property
                    {
                        FriendlyName = prop.Name,
                        Type = prop.PropertyType.Name
                    };
                }
                else
                {
                    var arguments = customAttr.ConstructorArguments;
                    var descriptionArgs = descriptionAttr?.ConstructorArguments;
                    var count = arguments.Count;
                    property = new Property()
                    {
                        FriendlyName = arguments[0].Value as string,
                        Regex = count > 2 ? arguments[1].Value as string : "",
                        Type = count > 1 ? arguments[2].Value as string : "",
                        Description = descriptionArgs?.Count > 0 ? descriptionArgs[0].Value as string : ""
                    };
                }

                if (prop.PropertyType.Namespace != "System")
                {
                    var typeDef = prop.PropertyType as TypeDefinition;
                    if (typeDef != null)
                    {
                        // Go through the properties
                        property.Properties = BuildProperties(typeDef.Properties);
                    }
                }

                yield return property;
            }
        }
    }
}