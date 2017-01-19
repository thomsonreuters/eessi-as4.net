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
            if (!Directory.Exists(folder)) throw new System.Exception($"The module folder {folder} doesn't exist");

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
            var infoAttribute = itemType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Infoattribute)?.ConstructorArguments;
            var descriptionAttribute = itemType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Descriptionattribute)?.ConstructorArguments;

            return new ItemType()
            {
                Name = infoAttribute == null ? itemType.Name : infoAttribute[0].Value as string,
                Description = descriptionAttribute == null ? string.Empty : descriptionAttribute.Count > 0 ? descriptionAttribute[0].Value as string : string.Empty,
                TechnicalName = $"{itemType.FullName}, {itemType.Module.Assembly.FullName}",
                Properties = properties
            };
        }

        private IEnumerable<Property> BuildProperties(Collection<PropertyDefinition> properties)
        {
            foreach (var prop in properties)
            {
                var customAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Infoattribute)?.ConstructorArguments;
                var descriptionAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Descriptionattribute)?.ConstructorArguments;
                Property property = new Property
                {
                    FriendlyName = customAttr != null ? customAttr[0].Value as string : prop.Name,
                    TechnicalName = prop.Name,
                    Regex = customAttr != null ? customAttr.Count > 1 ? customAttr[1].Value as string : string.Empty : string.Empty,
                    Type = customAttr != null && customAttr.Count > 2 ? customAttr[2].Value as string : prop.PropertyType.Name,
                    Description = descriptionAttr != null ? descriptionAttr.Count > 0 ? descriptionAttr[0].Value as string : string.Empty : string.Empty
                };

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