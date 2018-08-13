using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.ServiceHandler.Agents
{
    internal static class DefaultAgentReceiverRegistry
    {
        private static readonly IDictionary<AgentType, Receiver> Receivers =
            new Dictionary<AgentType, Receiver>();

        static DefaultAgentReceiverRegistry()
        {
            Receivers.Add(
                AgentType.Forward, 
                new Receiver
                {
                    Type = typeof(DatastoreReceiver).AssemblyQualifiedName,
                    Setting = CreateDatastoreSettings("OutMessages", Operation.ToBeForwarded, Operation.Forwarding)
                });

            Receivers.Add(
                AgentType.PushSend,
                new Receiver
                {
                    Type = typeof(DatastoreReceiver).AssemblyQualifiedName,
                    Setting = CreateDatastoreSettings("OutMessages", Operation.ToBeSent, Operation.Sending)
                });

            Receivers.Add(
                AgentType.OutboundProcessing,
                new Receiver
                {
                    Type = typeof(DatastoreReceiver).AssemblyQualifiedName,
                    Setting = CreateDatastoreSettings("OutMessages", Operation.ToBeProcessed, Operation.Processing)
                });

            Receivers.Add(
                AgentType.Deliver,
                new Receiver
                {
                    Type = typeof(DatastoreReceiver).AssemblyQualifiedName,
                    Setting = CreateDatastoreSettings("InMessages", Operation.ToBeDelivered, Operation.Delivering)
                });

            Receivers.Add(
                AgentType.Notify,
                new Receiver
                {
                    Type = typeof(DatastoreReceiver).AssemblyQualifiedName,
                    Setting = CreateDatastoreSettings(table: null, filter: Operation.ToBeNotified, update: Operation.Notifying)
                });
        }

        private static Setting[] CreateDatastoreSettings(string table, Operation filter, Operation update)
        {
            XmlAttribute fieldAttribute = new XmlDocument().CreateAttribute("Field");
            fieldAttribute.Value = "Operation";

            return new[]
            {
                new Setting("Table", table),
                new Setting("Filter", filter.ToString()),
                new Setting("Update", update.ToString())
                {
                    Attributes = new[] { fieldAttribute }
                }
            };
        }

        /// <summary>
        /// Gets the default <see cref="Receiver"/> for a requested <see cref="AgentType"/>.
        /// </summary>
        /// <param name="agentType"></param>
        /// <returns></returns>
        public static Receiver GetDefaultReceiverFor(AgentType agentType)
        {
            if (Receivers.ContainsKey(agentType))
            {
                return Receivers[agentType];
            }

            return null;
        }
    }
}
