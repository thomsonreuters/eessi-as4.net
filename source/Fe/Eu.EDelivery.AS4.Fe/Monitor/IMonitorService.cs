using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Monitor.Model;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Interface to implement a monitor service
    /// </summary>
    public interface IMonitorService
    {
        /// <summary>
        /// Gets the exceptions.
        /// </summary>
        /// <param name="filter">Exception filter object</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">filter - Filter must be supplied
        /// or
        /// Direction - Direction cannot be null</exception>
        /// <exception cref="Eu.EDelivery.AS4.Fe.BusinessException">Could not get any exceptions, something went wrong.</exception>
        Task<MessageResult<ExceptionMessage>> GetExceptions(ExceptionFilter filter);

        /// <summary>
        /// Gets the messages.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// filter - Filter cannot be null
        /// or
        /// Direction - Direction filter cannot be empty
        /// </exception>
        /// <exception cref="Eu.EDelivery.AS4.Fe.BusinessException">No messages found</exception>
        Task<MessageResult<Message>> GetMessages(MessageFilter filter);

        /// <summary>
        /// Gets the related messages.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        Task<MessageResult<Message>> GetRelatedMessages(Direction direction, string messageId);

        /// <summary>
        /// Gets the pmode number.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        string GetPmodeNumber(string pmode);

        /// <summary>
        /// Downloads the message body.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">messageId - messageId parameter cannot be null</exception>
        /// <exception cref="InvalidEnumArgumentException">direction</exception>
        Task<Stream> DownloadMessageBody(Direction direction, string messageId);

        /// <summary>
        /// Downloads the exception body.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">messageId - messageId parameter cannot be null</exception>
        Task<string> DownloadExceptionMessageBody(Direction direction, long id);

        /// <summary>
        /// Gets the exception detail.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <returns></returns>
        Task<string> GetExceptionDetail(Direction direction, long messageId);
    }
}