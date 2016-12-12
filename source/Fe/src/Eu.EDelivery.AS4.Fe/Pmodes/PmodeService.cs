using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using AutoMapper;
using EnsureThat;
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
        Task CreateReceiving(ReceivingPmode pmode);
        Task CreateSending(SendingPmode pmode);
        Task DeleteReceiving(string name);
        Task DeleteSending(string name);
        Task UpdateSending(SendingPmode pmode, string originalName);
        Task UpdateReceiving(ReceivingPmode pmode, string originalName);
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
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetReceivingByName(name);
        }

        public async Task<IEnumerable<string>> GetSendingNames()
        {
            return await source.GetSendingNames();
        }

        public async Task<SendingPmode> GetSendingByName(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            return await source.GetSendingByName(name);
        }

        public async Task CreateReceiving(ReceivingPmode pmode)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            var exists = await source.GetReceivingByName(pmode.Name);
            if (exists != null) throw new Exception($"Pmode with name {pmode.Name} already exists.");
            await source.CreateReceiving(pmode);
        }

        public async Task CreateSending(SendingPmode pmode)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            var exists = await source.GetSendingByName(pmode.Name);
            if (exists != null) throw new Exception($"Pmode with name {pmode.Name} already exists.");
            await source.CreateSending(pmode);
        }

        public async Task DeleteReceiving(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var exists = await source.GetReceivingByName(name);
            if (exists == null) throw new Exception($"Pmode with name {name} doesn't exist");
            await source.DeleteReceiving(name);
        }

        public async Task DeleteSending(string name)
        {
            EnsureArg.IsNotNullOrEmpty(name, nameof(name));
            var exists = await source.GetSendingByName(name);
            if (exists == null) throw new Exception($"Pmode with name {name} doesn't exist");
            await source.DeleteSending(name);
        }

        public async Task UpdateSending(SendingPmode pmode, string originalName)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (pmode.Name != originalName)
            {
                var newExists = await GetSendingByName(pmode.Name);
                if (newExists != null) throw new Exception($"Pmode with {originalName} already exists");
            }

            await source.UpdateSending(pmode, originalName);
        }

        public async Task UpdateReceiving(ReceivingPmode pmode, string originalName)
        {
            EnsureArg.IsNotNull(pmode, nameof(pmode));
            EnsureArg.IsNotNullOrEmpty(originalName, nameof(originalName));

            if (pmode.Name != originalName)
            {
                var newExists = await GetReceivingByName(pmode.Name);
                if (newExists != null) throw new Exception($"Pmode with {originalName} already exists");
            }

            await source.UpdateReceiving(pmode, originalName);
        }
    }

    public interface IAs4PmodeSource
    {
        Task<IEnumerable<string>> GetReceivingNames();
        Task<ReceivingPmode> GetReceivingByName(string name);
        Task<IEnumerable<string>> GetSendingNames();
        Task<SendingPmode> GetSendingByName(string name);
        Task CreateReceiving(ReceivingPmode pmode);
        Task DeleteReceiving(string name);
        Task DeleteSending(string name);
        Task CreateSending(SendingPmode pmode);
        Task UpdateSending(SendingPmode pmode, string originalName);
        Task UpdateReceiving(ReceivingPmode pmode, string originalName);
    }

    public class As4PmodeSource : IAs4PmodeSource
    {
        private readonly IOptions<PmodeSettings> settings;
        private readonly IMapper mapper;

        public As4PmodeSource(IOptions<PmodeSettings> settings, IMapper mapper)
        {
            this.settings = settings;
            this.mapper = mapper;
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

        public Task CreateReceiving(ReceivingPmode pmode)
        {
            var xmlSerializer = new XmlSerializer(typeof(ReceivingProcessingMode));
            var path = Path.Combine(settings.Value.ReceivingPmodeFolder, $"{pmode.Name}.xml");

            return Task.Factory.StartNew(() =>
            {
                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, pmode.Pmode);
                    File.WriteAllText(path, textWriter.ToString(), Encoding.Unicode);
                }
            });
        }

        public Task DeleteReceiving(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var path = Path.Combine(settings.Value.ReceivingPmodeFolder, $"{name}.xml");
                File.Delete(path);
            });
        }

        public Task DeleteSending(string name)
        {
            return Task.Factory.StartNew(() =>
            {
                var path = Path.Combine(settings.Value.SendingPmodeFolder, $"{name}.xml");
                File.Delete(path);
            });
        }

        public Task CreateSending(SendingPmode pmode)
        {
            var xmlSerializer = new XmlSerializer(typeof(SendingProcessingMode));
            var path = Path.Combine(settings.Value.SendingPmodeFolder, $"{pmode.Name}.xml");

            return Task.Factory.StartNew(() =>
            {
                using (var textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, pmode.Pmode);
                    File.WriteAllText(path, textWriter.ToString(), Encoding.Unicode);
                }
            });
        }

        public async Task UpdateSending(SendingPmode pmode, string originalName)
        {
            await CreateSending(pmode);
        }

        public async Task UpdateReceiving(ReceivingPmode pmode, string originalName)
        {
            await CreateReceiving(pmode);
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