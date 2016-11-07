using AutoMapper;

namespace Eu.EDelivery.AS4.Mappings.Notify
{
    /// <summary>
    /// Map a corresponding <see cref="Model.Core.Error"/> with <see cref="Model.Notify.NotifyMessage"/>
    /// </summary>
    public class ErrorToNotifyMap : Profile
    {
        public ErrorToNotifyMap()
        {
            CreateMap<Model.Core.Error, Model.Notify.NotifyMessage>()
                .AfterMap((error, notifyMessage) =>
                {
                    notifyMessage.MessageInfo.MessageId = error.MessageId;
                    notifyMessage.MessageInfo.RefToMessageId = error.RefToMessageId;

                    notifyMessage.StatusInfo.Status = error.IsFormedByException
                        ? Model.Notify.Status.Exception
                        : Model.Notify.Status.Error;
                })
                .ForAllOtherMembers(x => x.Ignore());
        }
    }
}