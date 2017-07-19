using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.Extensions.Options;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    /// <summary>
    /// Runtime load
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Runtime.IRuntimeLoader" />
    public class RuntimeLoader : IRuntimeLoader
    {
        private static readonly string RuntimeReceiverInterface = "Eu.EDelivery.AS4.Receivers.IReceiver";
        private static readonly string RuntimeStepsInterface = "Eu.EDelivery.AS4.Steps.IStep";
        private static readonly string RuntimeTransformerInterface = "Eu.EDelivery.AS4.Transformers.ITransformer";
        private static readonly string RuntimeCertificateRepositoryInterface = "Eu.EDelivery.AS4.Repositories.ICertificateRepository";
        private static readonly string RuntimeDeliverSenderInterface = "Eu.EDelivery.AS4.Strategies.Sender.IDeliverSender";
        private static readonly string RuntimePmodeInterface = "Eu.EDelivery.AS4.Model.PMode.IPMode";
        private static readonly string Infoattribute = typeof(InfoAttribute).Name;
        private static readonly string NoUiAttribute = typeof(NotConfigurableAttribute).Name;
        private static readonly string Descriptionattribute = "DescriptionAttribute";

        private readonly string folder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeLoader"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public RuntimeLoader(IOptions<ApplicationSettings> settings)
        {
            folder = settings.Value.Runtime;
            Initialize();
        }

        /// <summary>
        /// Gets the receivers.
        /// </summary>
        /// <value>
        /// The receivers.
        /// </value>
        public IEnumerable<ItemType> Receivers { get; private set; }
        /// <summary>
        /// Gets the steps.
        /// </summary>
        /// <value>
        /// The steps.
        /// </value>
        public IEnumerable<ItemType> Steps { get; private set; }
        /// <summary>
        /// Gets the transformers.
        /// </summary>
        /// <value>
        /// The transformers.
        /// </value>
        public IEnumerable<ItemType> Transformers { get; private set; }
        /// <summary>
        /// Gets the certificate repositories.
        /// </summary>
        /// <value>
        /// The certificate repositories.
        /// </value>
        public IEnumerable<ItemType> CertificateRepositories { get; private set; }
        /// <summary>
        /// Gets the deliver senders.
        /// </summary>
        /// <value>
        /// The deliver senders.
        /// </value>
        public IEnumerable<ItemType> DeliverSenders { get; private set; }
        /// <summary>
        /// Gets the receiving pmode.
        /// </summary>
        /// <value>
        /// The receiving pmode.
        /// </value>
        public IEnumerable<ItemType> ReceivingPmode { get; private set; }

        /// <summary>
        /// Load the information from the AS4 runtime
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IRuntimeLoader Initialize()
        {
            if (!Directory.Exists(folder)) throw new Exception($"The module folder {folder} doesn't exist");

            var types = LoadTypesFromAssemblies();
            Receivers = LoadImplementationsForType(types, RuntimeReceiverInterface);
            Steps = LoadImplementationsForType(types, RuntimeStepsInterface);
            Transformers = LoadImplementationsForType(types, RuntimeTransformerInterface);
            CertificateRepositories = LoadImplementationsForType(types, RuntimeCertificateRepositoryInterface);
            DeliverSenders = LoadImplementationsForType(types, RuntimeDeliverSenderInterface);
            ReceivingPmode = LoadImplementationsForType(types, RuntimePmodeInterface);

            return this;
        }

        /// <summary>
        /// Get all types from all assemblies.
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Get implemenation
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<ItemType> LoadImplementationsForType(List<TypeDefinition> types, string type)
        {
            var implementations = types
                .Where(x => x.Interfaces.Any(iface => iface.InterfaceType.FullName == type))
                .Where(x => !x.IsInterface)
                .Where(x => !x.IsAbstract)
                .Where(x => x.CustomAttributes.All(attr => attr.AttributeType.Name != NoUiAttribute));
            var itemTypes = implementations.Select(itemType => BuildItemType(itemType, BuildProperties(itemType.Properties)));
            return itemTypes.Where(x => x != null);
        }

        private ItemType BuildItemType(TypeDefinition itemType, IEnumerable<Property> properties)
        {
            var infoAttribute = itemType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Infoattribute)?.ConstructorArguments;
            var descriptionAttribute = itemType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Descriptionattribute)?.ConstructorArguments;

            return new ItemType
            {
                Name = infoAttribute == null ? itemType.Name : infoAttribute[0].Value as string,
                Description = descriptionAttribute == null ? string.Empty : descriptionAttribute.Count > 0 ? descriptionAttribute[0].Value as string : string.Empty,
                TechnicalName = $"{itemType.FullName}, {itemType.Module.Assembly.FullName}",
                Properties = properties
            };
        }

        private IEnumerable<Property> BuildProperties(Collection<PropertyDefinition> properties)
        {
            foreach (var prop in properties.Where(x => x.CustomAttributes.Any(y => y.AttributeType.Name == Infoattribute)))
            {
                var customAttr = prop.CustomAttributes.First(attr => attr.AttributeType.Name == Infoattribute)?.ConstructorArguments;
                var descriptionAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == Descriptionattribute)?.ConstructorArguments;
                var property = new Property
                {
                    FriendlyName = customAttr != null ? customAttr[0].Value as string : prop.Name,
                    TechnicalName = prop.Name,
                    Regex = customAttr != null ? customAttr.Count > 1 ? customAttr[1].Value as string : string.Empty : string.Empty,
                    Description = descriptionAttr != null ? descriptionAttr.Count > 0 ? descriptionAttr[0].Value as string : string.Empty : string.Empty,
                    Required = customAttr != null && (customAttr.Count >= 5 && Convert.ToBoolean(customAttr[4].Value))
                };

                var type = customAttr?.Count >= 2 ? customAttr[2].Value as string : null;
                property.Type = !string.IsNullOrEmpty(type) ? type : prop.PropertyType.Name.ToLower();

                var defaultValue = customAttr?.Count >= 4 ? customAttr[3].Value : null;
                if (defaultValue is CustomAttributeArgument defaultValueAttribute)
                {
                    property.DefaultValue = defaultValueAttribute.Value;
                }

                var attributeList = customAttr?.Count >= 6 ? customAttr[5].Value : null;
                if (attributeList is CustomAttributeArgument[] attributes)
                {
                    property.Attributes = attributes.Select(x => x.Value as string).ToList();
                }

                if (prop.PropertyType.Namespace != "System")
                {
                    var typeDef = prop.PropertyType as TypeDefinition;
                    if (typeDef != null)
                        property.Properties = BuildProperties(typeDef.Properties);
                }

                yield return property;
            }
        }
    }
}