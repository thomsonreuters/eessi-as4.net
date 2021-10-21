using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using log4net;
using Attachment = Eu.EDelivery.AS4.Model.Core.Attachment;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// <see cref="Attachment"/> Uploader to send E-Mail messages 
    /// with these <see cref="Attachment"/> Models
    /// </summary>
    [NotConfigurable]
    public class EmailAttachmentUploader : IAttachmentUploader
    {
        public const string Key = "EMAIL";

        private readonly IConfig _config;
        private Method _method;

        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        /// <summary>
        /// Initialize a new instance of the <see cref="EmailAttachmentUploader"/> class
        /// </summary>
        public EmailAttachmentUploader() : this(Config.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAttachmentUploader"/> class.
        /// </summary>
        /// <param name="config"></param>
        public EmailAttachmentUploader(IConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        /// <summary>
        /// Configure the <see cref="IAttachmentUploader"/>
        /// with a given <paramref name="payloadReferenceMethod"/>
        /// </summary>
        /// <param name="payloadReferenceMethod"></param>
        public void Configure(Method payloadReferenceMethod)
        {
            if (payloadReferenceMethod == null)
            {
                throw new ArgumentNullException(nameof(payloadReferenceMethod));
            }

            _method = payloadReferenceMethod;
        }

        /// <inheritdoc />
        public Task<UploadResult> UploadAsync(Attachment attachment, MessageInfo referringUserMessage)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (referringUserMessage == null)
            {
                throw new ArgumentNullException(nameof(referringUserMessage));
            }

            SendAttachmentAsMail(attachment);

            return Task.FromResult(UploadResult.SuccessWithId(payloadId: attachment.Id));
        }

        /// <summary>
        /// Start uploading <see cref="Attachment"/>
        /// </summary>
        /// <param name="attachment"></param>
        private void SendAttachmentAsMail(Attachment attachment)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(_config.GetSetting("smtpserver"));

            AddCommonInfoToMailMessage(mail);
            AddEMailAttachmentToMail(attachment, mail);
            AddSecurityToSmtpServer(smtpServer);

            smtpServer.Send(mail);

            LogUploadInformation(attachment);
        }

        private void AddCommonInfoToMailMessage(MailMessage mail)
        {
            mail.From = new MailAddress(_config.GetSetting("smtpusername"));

            AssignIfNotNull("body", body => mail.Body = body);
            AssignIfNotNull("subject", subject => mail.Subject = subject);
            AssignIfNotNull("to", to => mail.To.Add(to));
        }

        private void AssignIfNotNull(string key, Action<string> targetAction)
        {
            Parameter parameter = _method[key];
            if (parameter?.Value != null)
            {
                targetAction(parameter.Value);
            }
            else
            {
                Logger.Debug($"Following key is not defined in Paylaod Reference Method: {key}");
            }
        }

        private void AddSecurityToSmtpServer(SmtpClient smtpServer)
        {
            int.TryParse(_config.GetSetting("smtpport"), out int smtpServerPort);
            smtpServer.Port = smtpServerPort;

            SetNetWorkCredentials(smtpServer);
            smtpServer.EnableSsl = true;
        }

        private void SetNetWorkCredentials(SmtpClient smtpServer)
        {
            smtpServer.Credentials = new System.Net.NetworkCredential(
                _config.GetSetting("smtpusername"), _config.GetSetting("smtppassword"));
        }

        private static void AddEMailAttachmentToMail(Attachment attachment, MailMessage mail)
        {
            string extension = MimeTypeRepository.Instance.GetExtensionFromMimeType(attachment.ContentType);
            var emailAttachment = new System.Net.Mail.Attachment(attachment.Content, attachment.Id + extension);

            mail.Attachments.Add(emailAttachment);
        }

        private void LogUploadInformation(Attachment attachment)
        {
            string toEmailAddress = _method["to"]?.Value;
            Logger.Info($"Attachment {Config.Encode(attachment.Id)} is send as Mail Attachment to {Config.Encode(toEmailAddress)}");
        }
    }
}
