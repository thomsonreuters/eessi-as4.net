using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.PMode;
using Microsoft.Extensions.Options;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    public interface IAs4PmodeService
    {
        Task<IEnumerable<string>> GetReceivingNames();
        Task<ReceivingPmode> GetReceivingByName(string name);
        Task<IEnumerable<string>> GetSendingNames();
        Task<SendingPmode> GetSendingByName(string name);
    }

    public class As4PmodeService : IAs4PmodeService
    {
        private readonly IAs4PmodeSource source;

        public As4PmodeService(IAs4PmodeSource source)
        {
            this.source = source;
        }

        public async Task<IEnumerable<string>> GetReceivingNames()
        {
            return await source.GetReceivingNames();
        }

        public async Task<ReceivingPmode> GetReceivingByName(string name)
        {
            EnsureThat.EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var result = await source.GetReceivingByName(name);
            if (result == null) throw new Exception($"Receiving pmode -> {name} doesn't exist");
            return result;
        }

        public async Task<IEnumerable<string>> GetSendingNames()
        {
            return await source.GetSendingNames();
        }

        public async Task<SendingPmode> GetSendingByName(string name)
        {
            EnsureThat.EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var result = await source.GetSendingByName(name);
            if (result == null) throw new Exception($"Sending pmode -> {name} doesn't exist");
            return result;
        }
    }

    public interface IAs4PmodeSource
    {
        Task<IEnumerable<string>> GetReceivingNames();
        Task<ReceivingPmode> GetReceivingByName(string name);
        Task<IEnumerable<string>> GetSendingNames();
        Task<SendingPmode> GetSendingByName(string name);
    }

    public class As4PmodeSource : IAs4PmodeSource
    {
        private readonly IOptions<PmodeSettings> settings;

        public As4PmodeSource(IOptions<PmodeSettings> settings)
        {
            this.settings = settings;
        }

        public Task<IEnumerable<string>> GetReceivingNames()
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.ReceivingPmodeFolder, "*.xml")
                .Select(Path.GetFileNameWithoutExtension));
        }

        public Task<ReceivingPmode> GetReceivingByName(string name)
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.ReceivingPmodeFolder, "*.xml")
                .Where(file => Path.GetFileNameWithoutExtension(file) == name)
                .Select(pmode =>
                {
                    using (var reader = new FileStream(pmode, FileMode.Open))
                    {
                        var xml = new XmlSerializer(typeof(ReceivingProcessingMode));
                        var result = new ReceivingPmode
                        {
                            Name = Path.GetFileNameWithoutExtension(pmode),
                            Type = PmodeType.Receiving,
                            Pmode = (ReceivingProcessingMode)xml.Deserialize(reader)
                        };
                        return result;
                    }
                })
                .FirstOrDefault());
        }

        public Task<IEnumerable<string>> GetSendingNames()
        {
            return Task.Factory.StartNew(() => Directory
                .GetFiles(settings.Value.SendingPmodeFolder, "*.xml")
                .Select(Path.GetFileNameWithoutExtension));
        }

        public Task<SendingPmode> GetSendingByName(string name)
        {
            return Task.Factory.StartNew(() => Directory
               .GetFiles(settings.Value.SendingPmodeFolder, "*.xml")
               .Where(file => Path.GetFileNameWithoutExtension(file) == name)
               .Select(pmode =>
               {
                   using (var reader = new FileStream(pmode, FileMode.Open))
                   {
                       var xml = new XmlSerializer(typeof(SendingProcessingMode));
                       var result = new SendingPmode()
                       {
                           Name = Path.GetFileNameWithoutExtension(pmode),
                           Type = PmodeType.Sending,
                           Pmode = (SendingProcessingMode)xml.Deserialize(reader)
                       };
                       return result;
                   }
               })
               .FirstOrDefault());
        }
    }

    public class Pmode
    {
        public PmodeType Type { get; set; }
        public string Name { get; set; }
    }

    public class SendingPmode : Pmode
    {
        public SendingProcessingMode Pmode { get; set; }
    }

    public class ReceivingPmode : Pmode
    {
        public ReceivingProcessingMode Pmode { get; set; }
    }

    public enum PmodeType
    {
        Receiving,
        Sending
    }
}