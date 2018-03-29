using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Eu.EDelivery.AS4.Transformers;
using Microsoft.Extensions.Options;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    /// <summary>
    /// Runtime load
    /// </summary>
    /// <seealso cref="IRuntimeLoader" />
    public class RuntimeLoader : IRuntimeLoader
    {
        private static readonly string InfoAttribute = typeof(InfoAttribute).Name;
        private static readonly string NoUiAttribute = typeof(NotConfigurableAttribute).Name;
        private static readonly string DefaultValueAttribute = typeof(DefaultValueAttribute).Name;
        private static readonly string DescriptionAttribute = typeof(DescriptionAttribute).Name;
        private static readonly List<string> Attributes = new List<string> { InfoAttribute, NoUiAttribute, DefaultValueAttribute, DescriptionAttribute };

        private readonly string _folder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeLoader"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public RuntimeLoader(IOptions<ApplicationSettings> settings)
        {
            _folder = settings.Value.Runtime;
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
        public IEnumerable<ItemType> MetaData { get; private set; }

        /// <summary>
        /// Load the information from the AS4 runtime
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IRuntimeLoader Initialize()
        {
            if (!Directory.Exists(_folder)) throw new Exception($"The module folder {_folder} doesn't exist");

            List<TypeDefinition> types = LoadTypesFromAssemblies();
            Receivers = LoadImplementationsForType(types, typeof(IReceiver));
            Steps = LoadImplementationsForType(types, typeof(IStep));
            Transformers = LoadImplementationsForType(types, typeof(ITransformer));
            CertificateRepositories = LoadImplementationsForType(types, typeof(ICertificateRepository));
            DeliverSenders = LoadImplementationsForType(types, typeof(IDeliverSender));
            NotifySenders = LoadImplementationsForType(types, typeof(INotifySender));
            AttachmentUploaders = LoadImplementationsForType(types, typeof(IAttachmentUploader));
            DynamicDiscoveryProfiles = LoadImplementationsForType(types, typeof(IDynamicDiscoveryProfile));
            MetaData = LoadImplementationsForType(types, typeof(IPMode), false)
                .Concat(LoadImplementationsForType(types, typeof(Entities.SmpConfiguration), false));

            return this;
        }

        /// <summary>
        /// Get all types from all assemblies.
        /// </summary>
        /// <returns></returns>
        public List<TypeDefinition> LoadTypesFromAssemblies()
        {
            return Directory
                    .GetFiles(_folder)
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
        /// <param name="onlyWithAttribute">Indicates that when building the properties only properties decorated with an attribute should be scanned.</param>
        /// <returns></returns>
        public IEnumerable<ItemType> LoadImplementationsForType(
            List<TypeDefinition> types,
            Type type,
            bool onlyWithAttribute = true)
        {
            bool TypeImplementsInterface(TypeDefinition x) =>
                x.Interfaces.Any(iface => iface.InterfaceType.FullName == type.FullName)
                || ImplementsRootInterface(x, type);

            ItemType ToItemType(TypeDefinition t) => 
                BuildItemType(t, BuildProperties(t.Properties, t.Name, onlyWithAttribute));

            return types
                .Where(TypeImplementsInterface)
                .Where(x => !x.IsInterface && !x.IsAbstract && x.IsPublic)
                .Where(x => x.CustomAttributes.All(attr => attr.AttributeType.Name != NoUiAttribute))
                .Select(ToItemType)
                .Where(x => x != null);
        }

        private static bool ImplementsRootInterface(TypeDefinition x, Type parent)
        {
            if (!x.FullName.Contains("AS4") || x.FullName.Contains("Test"))
            {
                return false;
            }

            try
            {
                Type child = Assembly
                    .LoadFile(Path.GetFullPath(x.Module.FileName))
                    .GetType(x.FullName);

                return parent.IsAssignableFrom(child);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private ItemType BuildItemType(TypeDefinition itemType, IEnumerable<Property> properties)
        {
            Collection<CustomAttributeArgument> infoAttribute = itemType.CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType.Name == InfoAttribute)
                ?.ConstructorArguments;

            Collection<CustomAttributeArgument> descriptionAttribute = itemType.CustomAttributes
                .FirstOrDefault(attr => attr.AttributeType.Name == DescriptionAttribute)
                ?.ConstructorArguments;

            return new ItemType
            {
                Name = infoAttribute == null ? itemType.Name : infoAttribute[0].Value as string,
                Description =
                    descriptionAttribute == null
                        ? string.Empty
                        : descriptionAttribute.Count > 0
                            ? descriptionAttribute[0].Value as string
                            : string.Empty,
                TechnicalName = $"{itemType.FullName}, {itemType.Module.Assembly.FullName}",
                Properties = properties
            };
        }

        private static IEnumerable<Property> BuildProperties(
            IEnumerable<PropertyDefinition> properties, 
            string propPath, 
            bool onlyWithAttribute = true)
        {
            if (onlyWithAttribute)
            {
                properties = properties.Where(x => x.CustomAttributes.Any(y => Attributes.Contains(y.AttributeType.Name)));
            }

            foreach (PropertyDefinition prop in properties)
            {
                Collection<CustomAttributeArgument> descriptionAttr = prop.CustomAttributes
                    .FirstOrDefault(attr => attr.AttributeType.Name == DescriptionAttribute)
                    ?.ConstructorArguments;

                var property = new Property
                {
                    Type = prop.PropertyType.Name,
                    TechnicalName = prop.Name,
                    Description =
                        descriptionAttr != null
                            ? descriptionAttr.Count > 0
                                  ? descriptionAttr[0].Value as string
                                  : string.Empty
                            : string.Empty,
                    Path = string.IsNullOrEmpty(propPath)
                               ? prop.Name.ToLower()
                               : propPath.ToLower() + "." + prop.Name.ToLower()
                };

                ApplyDefaultValueAttribute(property, prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == DefaultValueAttribute)?.ConstructorArguments);
                ApplyInfoAttribute(property, prop, prop.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.Name == InfoAttribute)?.ConstructorArguments);

                if (prop.PropertyType.Namespace != "System")
                {
                    if (prop.PropertyType is TypeDefinition typeDef)
                    {
                        IEnumerable<Property> BuildProps(TypeDefinition type, string path)
                        {
                            IEnumerable<Property> xs = 
                                BuildProperties(type.Properties, path + "." + prop.Name.ToLower(), onlyWithAttribute);

                            if (type.BaseType is TypeDefinition typedef && typedef.HasProperties)
                            {
                               return xs.Concat(
                                   BuildProperties(typedef.Properties, path + "." + prop.Name.ToLower(), onlyWithAttribute));
                            }

                            return xs;
                        }

                        property.Properties = BuildProps(typeDef, propPath.ToLower()).Distinct().ToList();
                    }


                }

                yield return property;
            }
        }

        private static void ApplyInfoAttribute(
            Property property,
            PropertyDefinition prop,
            IList<CustomAttributeArgument> arguments)
        {
            if (arguments == null)
            {
                return;
            }

            property.FriendlyName = arguments[0].Value as string ?? prop.Name;
            property.Regex = arguments.Count > 1 ? arguments[1].Value as string : string.Empty;
            property.Required = arguments.Count >= 5 && Convert.ToBoolean(arguments[4].Value);

            string type = arguments.Count >= 2 ? arguments[2].Value as string : null;
            property.Type = !string.IsNullOrEmpty(type) ? type : prop.PropertyType.Name.ToLower();

            object defaultValue = arguments.Count >= 4 ? arguments[3].Value : null;
            if (defaultValue is CustomAttributeArgument defaultValueAttribute)
            {
                property.DefaultValue = defaultValueAttribute.Value;
            }

            object attributeList = arguments.Count >= 6 ? arguments[5].Value : null;
            if (attributeList is CustomAttributeArgument[] attributes)
            {
                property.Attributes = attributes.Select(x => x.Value as string).ToList();
            }
        }

        private static void ApplyDefaultValueAttribute(
            Property property,
            IList<CustomAttributeArgument> arguments)
        {
            if (arguments == null)
            {
                return;
            }

            if (arguments[0].Value is CustomAttributeArgument argument)
            {
                property.DefaultValue = argument.Value;
            }
            else
            {
                property.DefaultValue = arguments[0].Value;
            }
        }
    }
}