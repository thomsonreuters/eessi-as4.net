using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.Strategies.Uploader;
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
        private static readonly string RuntimeNotifySenderInterface = "Eu.EDelivery.AS4.Strategies.Sender.INotifySender";
        private static readonly string RuntimeAttachmentUploaderInterface = typeof(IAttachmentUploader).FullName;
        private static readonly string RuntimeDynamicDiscoveryProfileInterface = typeof(IDynamicDiscoveryProfile).FullName;
        private static readonly string RuntimePmodeInterface = "Eu.EDelivery.AS4.Model.PMode.IPMode";
        private static readonly string InfoAttribute = typeof(InfoAttribute).Name;
        private static readonly string NoUiAttribute = typeof(NotConfigurableAttribute).Name;
        private static readonly string DefaultValueAttribute = typeof(DefaultValueAttribute).Name;
        private static readonly string DescriptionAttribute = typeof(DescriptionAttribute).Name;
        private static readonly List<string> Attributes = new List<string> { InfoAttribute, NoUiAttribute, DefaultValueAttribute, DescriptionAttribute };

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
        /// Gets the notify senders.
        /// </summary>
        /// <value>
        /// The notify senders.
        /// </value>
        public IEnumerable<ItemType> NotifySenders { get; private set; }
        /// <summary>
        /// Gets the attachment uploaders.
        /// </summary>
        /// <value>
        /// The attachment uploaders.
        /// </value>
        public IEnumerable<ItemType> AttachmentUploaders { get; private set; }
        /// <summary>
        /// Gets the dynamic discovery profiles.
        /// </summary>
        /// <value>
        /// The dynamic discovery profiles.
        /// </value>
        public IEnumerable<ItemType> DynamicDiscoveryProfiles { get; private set; }
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
            NotifySenders = LoadImplementationsForType(types, RuntimeNotifySenderInterface);
            AttachmentUploaders = LoadImplementationsForType(types, RuntimeAttachmentUploaderInterface);
            DynamicDiscoveryProfiles = LoadImplementationsForType(types, RuntimeDynamicDiscoveryProfileInterface);
            ReceivingPmode = LoadImplementationsForType(types, RuntimePmodeInterface, false);

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
                        return file != "libuv.dll" && !file.StartsWith("Microsoft") && !file.StartsWith("System") && file != "sqlite3.dll" && !file.Contains("Test");
                    })
                    .SelectMany(file => AssemblyDefinition.ReadAssembly(file).MainModule.Types)
                    .ToList();
        }


        /// <summary>
        /// Get implemenation
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="type">The type.</param>
        /// <param name="onlyWithAttribute">Indicates that when building the properties only properties decorated with an attribute should be scanned.</param>
        /// <returns></returns>
        public IEnumerable<ItemType> LoadImplementationsForType(List<TypeDefinition> types, string type, bool onlyWithAttribute = true)
        {
            var implementations = types
                .Where(x => x.Interfaces.Any(iface => iface.InterfaceType.FullName == type))
                .Where(x => !x.IsInterface)
                .Where(x => !x.IsAbstract)
                .Where(x => x.CustomAttributes.All(attr => attr.AttributeType.Name != NoUiAttribute));
            var itemTypes = implementations.Select(itemType => BuildItemType(itemType, BuildProperties(itemType.Properties, itemType.Name, onlyWithAttribute)));
            return itemTypes.Where(x => x != null);
        }

        private ItemType BuildItemType(TypeDefinition itemType, IEnumerable<Property> properties)
        {
            var infoAttribute = itemType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == InfoAttribute)?.ConstructorArguments;
            var descriptionAttribute = itemType.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == DescriptionAttribute)?.ConstructorArguments;

            return new ItemType
            {
                Name = infoAttribute == null ? itemType.Name : infoAttribute[0].Value as string,
                Description = descriptionAttribute == null ? string.Empty : descriptionAttribute.Count > 0 ? descriptionAttribute[0].Value as string : string.Empty,
                TechnicalName = $"{itemType.FullName}, {itemType.Module.Assembly.FullName}",
                Properties = properties
            };
        }

        private void ApplyInfoAttribute(Property property, PropertyDefinition prop, Collection<CustomAttributeArgument> arguments)
        {
            if (arguments == null) return;

            property.FriendlyName = arguments[0].Value as string ?? prop.Name;
            property.Regex = arguments.Count > 1 ? arguments[1].Value as string : string.Empty;
            property.Required = arguments.Count >= 5 && Convert.ToBoolean(arguments[4].Value);

            var type = arguments.Count >= 2 ? arguments[2].Value as string : null;
            property.Type = !string.IsNullOrEmpty(type) ? type : prop.PropertyType.Name.ToLower();

            var defaultValue = arguments.Count >= 4 ? arguments[3].Value : null;
            if (defaultValue is CustomAttributeArgument defaultValueAttribute)
            {
                property.DefaultValue = defaultValueAttribute.Value;
            }

            var attributeList = arguments.Count >= 6 ? arguments[5].Value : null;
            if (attributeList is CustomAttributeArgument[] attributes)
            {
                property.Attributes = attributes.Select(x => x.Value as string).ToList();
            }
        }

        private void ApplyDefaultValueAttribute(Property property, Collection<CustomAttributeArgument> arguments)
        {
            if (arguments == null) return;

            if (arguments[0].Value is CustomAttributeArgument argument)
            {
                property.DefaultValue = argument.Value;
            }
            else
            {
                property.DefaultValue = arguments[0].Value;
            }
        }

        private IEnumerable<Property> BuildProperties(IEnumerable<PropertyDefinition> properties, string propPath, bool onlyWithAttribute = true)
        {
            if (onlyWithAttribute) properties = properties.Where(x => x.CustomAttributes.Any(y => Attributes.Contains(y.AttributeType.Name)));

            foreach (var prop in properties)
            {
                var descriptionAttr = prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == DescriptionAttribute)?.ConstructorArguments;
                var property = new Property
                {
                    Type = prop.PropertyType.Name,
                    TechnicalName = prop.Name,
                    Description = descriptionAttr != null ? descriptionAttr.Count > 0 ? descriptionAttr[0].Value as string : string.Empty : string.Empty,
                    Path = string.IsNullOrEmpty(propPath) ? prop.Name.ToLower() : propPath.ToLower() + "." + prop.Name.ToLower()
                };

                ApplyDefaultValueAttribute(property, prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == DefaultValueAttribute)?.ConstructorArguments);
                ApplyInfoAttribute(property, prop, prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == InfoAttribute)?.ConstructorArguments);

                if (prop.PropertyType.Namespace != "System")
                {
                    var typeDef = prop.PropertyType as TypeDefinition;
                    if (typeDef != null)
                    {
                        IEnumerable<Property> BuildProps(TypeDefinition type, string path)
                        {
                            foreach (var childProp in BuildProperties(type.Properties, path + "." + prop.Name.ToLower(), onlyWithAttribute))
                            {
                                yield return childProp;
                            }

                            if (type.BaseType is TypeDefinition typedef && typedef.HasProperties)
                            {
                                foreach (var childProp in BuildProperties(typedef.Properties, path + "." + prop.Name.ToLower(), onlyWithAttribute))
                                {
                                    yield return childProp;
                                }
                            }
                        }
                        property.Properties = BuildProps(typeDef, propPath.ToLower()).Distinct().ToList();
                    }
                }

                yield return property;
            }
        }
    }
}